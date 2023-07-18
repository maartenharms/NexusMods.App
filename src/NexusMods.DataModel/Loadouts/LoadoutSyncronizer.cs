using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using DynamicData;
using Microsoft.Extensions.Logging;
using NexusMods.Common;
using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.Extensions;
using NexusMods.DataModel.Games;
using NexusMods.DataModel.Loadouts.ApplySteps;
using NexusMods.DataModel.Loadouts.IngestSteps;
using NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs;
using NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;
using NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates.Failures;
using NexusMods.DataModel.Loadouts.ModFiles;
using NexusMods.DataModel.Loadouts.Mods;
using NexusMods.DataModel.Sorting;
using NexusMods.DataModel.Sorting.Rules;
using NexusMods.DataModel.TriggerFilter;
using NexusMods.FileExtractor.StreamFactories;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;
using BackupFile = NexusMods.DataModel.Loadouts.ApplySteps.BackupFile;

namespace NexusMods.DataModel.Loadouts;

/// <summary>
/// All logic for synchronizing loadouts with the game folders is contained within this class.
/// </summary>
public class LoadoutSynchronizer
{
    private readonly IFingerprintCache<Mod,CachedModSortRules> _modSortRulesFingerprintCache;
    private readonly IDirectoryIndexer _directoryIndexer;
    private readonly IArchiveManager _archiveManager;
    private readonly IFingerprintCache<IGeneratedFile,CachedGeneratedFileData> _generatedFileFingerprintCache;
    private readonly LoadoutRegistry _loadoutRegistry;
    private readonly ILogger<LoadoutSynchronizer> _logger;

    /// <summary>
    /// DI constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="modSortRulesFingerprintCache"></param>
    /// <param name="directoryIndexer"></param>
    /// <param name="archiveManager"></param>
    /// <param name="generatedFileFingerprintCache"></param>
    /// <param name="loadoutRegistry"></param>
    public LoadoutSynchronizer(ILogger<LoadoutSynchronizer> logger, 
        IFingerprintCache<Mod, CachedModSortRules> modSortRulesFingerprintCache,
        IDirectoryIndexer directoryIndexer,
        IArchiveManager archiveManager,
        IFingerprintCache<IGeneratedFile, CachedGeneratedFileData> generatedFileFingerprintCache,
        LoadoutRegistry loadoutRegistry)
    {
        _logger = logger;
        _archiveManager = archiveManager;
        _generatedFileFingerprintCache = generatedFileFingerprintCache;
        _modSortRulesFingerprintCache = modSortRulesFingerprintCache;
        _directoryIndexer = directoryIndexer;
        _loadoutRegistry = loadoutRegistry;
    }


    /// <summary>
    /// Flattens a loadout into a dictionary of files and their corresponding mods. Any files that are not
    /// IToFile will be ignored.
    /// </summary>
    /// <param name="loadout"></param>
    /// <returns></returns>
    public async ValueTask<FlattenedLoadout> FlattenLoadout(Loadout loadout)
    {
        var files = new Dictionary<GamePath, ModFilePair>();

        var sorted = (await SortMods(loadout)).ToList();

        foreach (var mod in sorted)
        {
            foreach (var (_, file) in mod.Files)
            {
                if (file is not IToFile toFile)
                    continue;

                files[toFile.To] = new ModFilePair {Mod = mod, File = file};
            }
        }

        return new FlattenedLoadout
        {
            Files = files,
            Mods = sorted
        };
    }


    /// <summary>
    /// Sorts the mods in the given loadout, using the rules defined in the mods.
    /// </summary>
    /// <param name="loadout"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Mod>> SortMods(Loadout loadout)
    {
        var modRules = await loadout.Mods.Values
            .SelectAsync(async mod => (mod.Id, await ModSortRules(loadout, mod).ToListAsync()))
            .ToDictionaryAsync(r => r.Id, r => r.Item2);
        var sorted = Sorter.Sort<Mod, ModId>(loadout.Mods.Values.ToList(), m => m.Id, m => modRules[m.Id]);
        return sorted;
    }

    /// <summary>
    /// Generates a list of Rules for sorting the given mod, if the mod has any generated rules then they are
    /// calculated and returned.
    /// </summary>
    /// <param name="loadout"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<ISortRule<Mod, ModId>> ModSortRules(Loadout loadout, Mod mod)
    {
        foreach (var rule in mod.SortRules)
        {
            if (rule is IGeneratedSortRule gen)
            {
                var fingerprint = gen.TriggerFilter.GetFingerprint(mod.Id, loadout);
                if (_modSortRulesFingerprintCache.TryGet(fingerprint, out var cached))
                {
                    foreach (var cachedRule in cached.Rules)
                        yield return cachedRule;
                    continue;
                }

                var rules = await gen.GenerateSortRules(mod.Id, loadout).ToArrayAsync();
                _modSortRulesFingerprintCache.Set(fingerprint, new CachedModSortRules
                {
                    Rules = rules
                });

                foreach (var genRule in rules)
                {
                    yield return genRule;
                }
            }
            else
            {
                yield return rule;
            }
        }
    }

    /// <summary>
    /// Validates the given loadout, checking that it can be applied, given the current state of the game folders.
    /// </summary>
    /// <param name="loadout"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async ValueTask<AValidationResult> Validate(Loadout loadout, CancellationToken token = default)
    {
        return await Validate(new BaseConfiguration
        {
            PlannedState = loadout
        }, token);
    }

    /// <summary>
    /// Validates the given base configuration, checking that it can be applied, given the current state of the game folders.
    /// </summary>
    /// <param name="baseConfiguration"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async ValueTask<AValidationResult> Validate(BaseConfiguration baseConfiguration, CancellationToken token)
    {
        var plannedState = baseConfiguration.PlannedState;
        var appliedState = _loadoutRegistry.GetLastApplied(plannedState);
        var flattenedPlannedState = await FlattenLoadout(plannedState);
        
        if (appliedState == null)
        {
            return new NoPreviouslyAppliedState()
            {
                PlannedState = plannedState,
            };
        }


        var flattenedAppliedState = await FlattenLoadout(appliedState);
        var existingFiles = _directoryIndexer.IndexFolders(plannedState.Installation.Locations.Values, token);
        
        var conflicts = new List<HashedEntry>();
        var diskState = new Dictionary<GamePath, HashedEntry>();
        var appliedMetas = new Dictionary<GamePath, FileMetaData>();
        var appliedFingerprintingState = new FingerprintingValidationState
        {
            Loadout = appliedState,
            FlattenedLoadout = flattenedAppliedState
        };
        
        await foreach (var file in existingFiles.WithCancellation(token))
        {
            var gamePath = plannedState.Installation.ToGamePath(file.Path);
            diskState.Add(gamePath, file);
            if (flattenedAppliedState.Files.TryGetValue(gamePath, out var foundFilePair))
            {
                var appliedMetaData = GetMetaData(foundFilePair, file.Path, appliedFingerprintingState);
                appliedMetas[gamePath] = appliedMetaData;
                if (appliedMetaData.Hash != file.Hash || appliedMetaData.Size != file.Size)
                {
                    conflicts.Add(file);
                }
                
            }
            else
            {
                conflicts.Add(file);
            }
        }

        if (conflicts.Any())
        {
            return new UningestedFiles
            {
                PlannedState = plannedState,
                AppliedState = appliedState,
                Conflicts = conflicts,
                DiskState = new ReadOnlyDictionary<GamePath, HashedEntry>(diskState),
                FlattenedAppliedState = flattenedAppliedState,
                FlattenedPlannedState = flattenedPlannedState
            };
        }
        
        // From now on we know that the disk state is in sync with the applied state
        // so we can use the disk state and the applied state interchangeably
        
        var toExtract = new Dictionary<AbsolutePath, Hash>();
        var toGenerate = new Dictionary<AbsolutePath, GeneratedFileState>();
        var toDelete = new HashSet<AbsolutePath>();

        var plannedFingerprintingState = new FingerprintingValidationState
        {
            Loadout = plannedState,
            FlattenedLoadout = flattenedPlannedState
        };

        void AddToCreate(GamePath path, ModFilePair pair)
        {
            var absPath = path.CombineChecked(plannedState.Installation);
            switch (pair.File)
            {
                case IFromArchive fromArchive:
                    toExtract!.Add(absPath, fromArchive.Hash);
                    break;
                case IGeneratedFile generatedFile:
                    var fingerprint = generatedFile.TriggerFilter.GetFingerprint(pair, plannedFingerprintingState);
                    toGenerate!.Add(absPath, new GeneratedFileState()
                    {
                        GeneratedFile = generatedFile,
                        ModFilePair = pair,
                        Fingerprint = fingerprint
                    });
                    break;
            }
        }


        // Files that are no longer needed in the game folder should be deleted
        foreach (var (path, _) in flattenedAppliedState.Files)
        {
            if (!flattenedPlannedState.Files.ContainsKey(path))
            {
                toDelete.Add(diskState[path].Path);
            }
        }
        


        // Files that are in the planned state but not in the applied state should be created or replaced
        foreach (var (path, pair) in flattenedPlannedState.Files)
        {
            var absPath = path.CombineChecked(plannedState.Installation);
            if (flattenedAppliedState.Files.ContainsKey(path))
            {
                var appliedMeta = appliedMetas[path];
                var plannedMeta = GetMetaData(pair, absPath, plannedFingerprintingState);
                if (appliedMeta.Fingerprint != null && plannedMeta.Fingerprint != null)
                {
                    if (appliedMeta.Fingerprint.Value == plannedMeta.Fingerprint.Value)
                        continue;
                }
                else
                {
                    if (appliedMeta.Size == plannedMeta.Size && appliedMeta.Hash == plannedMeta.Hash)
                        continue;
                }
                toDelete.Add(absPath);
                AddToCreate(path, pair);
            }
            else
            {
                AddToCreate(path, pair);
            }
        }
        
        return new SuccessfulValidationResult
        {
            PlannedState = plannedState,
            AppliedState = appliedState,
            DiskState = new ReadOnlyDictionary<GamePath, HashedEntry>(diskState),
            FlattenedAppliedState = flattenedAppliedState,
            FlattenedPlannedState = flattenedPlannedState,
            ToExtract = new ReadOnlyDictionary<AbsolutePath, Hash>(toExtract),
            ToGenerate = toGenerate,
            ToDelete = toDelete
        };
    }


    /// <summary>
    /// Gets the metadata for the given file, if the file is from an archive then the metadata is returned
    /// </summary>
    /// <param name="pair"></param>
    /// <param name="path"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public FileMetaData GetMetaData(ModFilePair pair, AbsolutePath path, FingerprintingValidationState state)
    {
        switch (pair.File)
        {
            case IFromArchive fa:
                return new FileMetaData(path, fa.Hash, fa.Size);
            case IGeneratedFile gf:
            {
                var fingerprint = gf.TriggerFilter.GetFingerprint(pair, state);
                if (_generatedFileFingerprintCache.TryGet(fingerprint, out var cached))
                {
                    return new FileMetaData(path, cached.Hash, cached.Size);
                }
                else
                {
                    return new FileMetaData(path, Fingerprint:fingerprint);
                }
            }
            default:
                throw new NotImplementedException("Unknown file type, this should never happen");
        }
    }

    /// <summary>
    /// Compares the game folders to the loadout and returns a plan of what needs to be done to make the loadout match the game folders
    /// </summary>
    /// <param name="loadout"></param>
    /// <param name="modSelector"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async ValueTask<AValidationResult> MakeIngestPlan(Loadout plannedState, Func<AbsolutePath, ModId> modSelector, CancellationToken token = default)
    {
        ModFilePair? appliedFile ;
        
        var install = plannedState.Installation;
        var appliedState = _loadoutRegistry.GetLastApplied(plannedState);
        if (appliedState == null)
        {
            return new NoPreviouslyAppliedState
            {
                PlannedState = plannedState,
            };
        }
        
        var flattenedPlannedState = await FlattenLoadout(plannedState);
        var flattenedAppliedState = await FlattenLoadout(appliedState!);
        var diskState = new Dictionary<GamePath, HashedEntry>();
        var existingFiles = _directoryIndexer.IndexFolders(install.Locations.Values, token);

        var seen = new ConcurrentBag<GamePath>();
        
        var appliedFingerprintingState = new FingerprintingValidationState
        {
            Loadout = appliedState,
            FlattenedLoadout = flattenedAppliedState
        };
        
        var plannedFingerprintingState = new FingerprintingValidationState
        {
            Loadout = plannedState,
            FlattenedLoadout = flattenedPlannedState
        };

        // Files that don't require a fork
        var toAdd = new Dictionary<AbsolutePath, ModId>();
        var toUpdate = new Dictionary<AbsolutePath, ModFilePair>();
        var toParse = new Dictionary<AbsolutePath, ModFilePair>();
        var toDelete = new HashSet<GamePath>();
        var isForked = false;

        
        await foreach (var existing in existingFiles.WithCancellation(token))
        {
            var gamePath = install.ToGamePath(existing.Path);
            seen.Add(gamePath);
            diskState.Add(gamePath, existing);

            // Is the file in the planned state?
            if (flattenedPlannedState.Files.TryGetValue(gamePath, out var plannedFile))
            {
                // It is, so see if there is a conflict
                
                // If the file is IFromArchive
                if (plannedFile.File is IFromArchive fromArchive)
                {
                    if (fromArchive.Hash == existing.Hash && fromArchive.Size == existing.Size)
                        // They're the same, so we can ignore it
                        continue;
                    
                    toUpdate.Add(existing.Path, plannedFile);
                    continue;
                }

                // Is the planned file a IGeneratedFile?
                if (plannedFile.File is IGeneratedFile generatedFile)
                {
                    // See if we have cached metadata for some reason
                    var fingerprint = generatedFile.TriggerFilter.GetFingerprint(plannedFile, plannedFingerprintingState);
                    if (_generatedFileFingerprintCache.TryGet(fingerprint, out var cached))
                    {
                        if (cached.Hash == existing.Hash && cached.Size == existing.Size)
                            // They're the same, so we can ignore it
                            continue;
                    }
                    if (flattenedAppliedState.Files.TryGetValue(gamePath, out var appliedFile3))
                    {
                        if (appliedFile3.File is IGeneratedFile appliedGeneratedFile)
                        {
                            var appliedFingerprint = appliedGeneratedFile.TriggerFilter.GetFingerprint(appliedFile3, appliedFingerprintingState);
                            if (appliedFingerprint == fingerprint && _generatedFileFingerprintCache.TryGet(appliedFingerprint, out var appledCached))
                            {
                                if (appledCached.Hash == existing.Hash && appledCached.Size == existing.Size)
                                    // The file is the same as the applied state, and the planned fingerprint matches
                                    // the applied fingerprint so we're all good and can skip this file.
                                    continue;
                            }
                        }
                    }
                    // We need to parse the file and fork the modlist
                    toParse.Add(existing.Path, plannedFile);
                    continue;
                }

                throw new NotImplementedException("Unknown file type, this should never happen: + " + plannedFile.File.GetType().FullName);
            }
            
            

            // File isn't in the planned state, so we need to get rid of it, is it in the applied state?
            if (flattenedAppliedState.Files.TryGetValue(gamePath, out appliedFile))
            {
                // It is, so see if it's changed
                var appliedMetaData = GetMetaData(appliedFile, existing.Path, appliedFingerprintingState);
                if (appliedMetaData.Hash == existing.Hash && appliedMetaData.Size == existing.Size)
                    // They're the same, and the file doesn't exist in the planned state, so we can ignore it
                    continue;
                
                // The file has changed, so we need to back it up and fork the loadout
                var mod = modSelector(existing.Path);
                toAdd.Add(existing.Path, mod);
                isForked = true;
                continue;
            }
        }

        foreach (var (gamePath, pair) in flattenedPlannedState.Files)
        {
            if (seen.Contains(gamePath))
                continue;
            // This file wasn't on the disk, so we need to delete it from the planned state
            toDelete.Add(gamePath);
        }
            

        return new SuccessfulIngest
        {
            PlannedState = plannedState,
            AppliedState = appliedState,
            FlattenedAppliedState = flattenedAppliedState,
            FlattenedPlannedState = flattenedPlannedState,
            DiskState = new ReadOnlyDictionary<GamePath, HashedEntry>(diskState),
            ToAdd = toAdd,
            ToDelete = toDelete,
            ToUpdate = toUpdate,
            ToParse = toParse,
            IsForking = isForked,
        };
    }

    private ValueTask EmitRemoveFromLoadout(List<IIngestStep> plan, AbsolutePath absPath)
    {
        plan.Add(new IngestSteps.RemoveFromLoadout
        {
            Source = absPath
        });
        return ValueTask.CompletedTask;
    }

    private async ValueTask EmitIngestCreatePlan(List<IIngestStep> plan, HashedEntry existing, Func<AbsolutePath, ModId> modSelector)
    {
        if (!await _archiveManager.HaveFile(existing.Hash))
        {
            plan.Add(new IngestSteps.BackupFile
            {
                Source = existing.Path,
                Hash = existing.Hash,
                Size = existing.Size
            });
        }

        plan.Add(new CreateInLoadout
        {
            Source = existing.Path,
            Hash = existing.Hash,
            Size = existing.Size,
            ModId = modSelector(existing.Path)
        });
    }

    private async ValueTask EmitIngestReplacePlan(List<IIngestStep> plan, ModFilePair pair, HashedEntry existing)
    {
        if (!await _archiveManager.HaveFile(existing.Hash))
        {
            plan.Add(new IngestSteps.BackupFile
            {
                Source = existing.Path,
                Hash = existing.Hash,
                Size = existing.Size
            });
        }

        plan.Add(new ReplaceInLoadout
        {
            Source = existing.Path,
            Hash = existing.Hash,
            Size = existing.Size,
            ModFileId = pair.File.Id,
            ModId = pair.Mod.Id
        });
    }

    /// <summary>
    /// Applies the given steps to the game folder
    /// </summary>
    /// <param name="plan"></param>
    /// <param name="token"></param>
    public async Task Apply(SuccessfulValidationResult plan, CancellationToken token = default)
    {
        // Step 1: Delete Files
        foreach (var path in plan.ToDelete)
        {
            path.Delete();
        }
        
        // Step 3: Extract Files
        var extractedFiles = plan.ToExtract
            .Select(kv => (kv.Value, kv.Key));
        await _archiveManager.ExtractFiles(extractedFiles, token);

        // Step 4: Write Generated Files
        var plannedFingerprintingState = new FingerprintingValidationState
        {
            Loadout = plan.PlannedState,
            FlattenedLoadout = plan.FlattenedPlannedState
        };
        
        foreach (var (absPath, generatedFile) in plan.ToGenerate)
        {
            var dir = absPath.Parent;
            if (!dir.DirectoryExists())
                dir.CreateDirectory();

            await using var stream = absPath.Create();
            var hash = await generatedFile.GeneratedFile.GenerateAsync(stream, plannedFingerprintingState, token);
            _generatedFileFingerprintCache.Set(generatedFile.Fingerprint, new CachedGeneratedFileData
            {
                Hash = hash,
                Size = Size.FromLong(stream.Length)
            });
        }
        
        // Step 5: Update Loadout last applied state
        _loadoutRegistry.SetLastApplied(plan.PlannedState);
    }

    /// <summary>
    /// Run an ingest plan
    /// </summary>
    /// <param name="plan"></param>
    /// <param name="commitMessage"></param>
    public async ValueTask<Loadout> Ingest(SuccessfulIngest plan, string commitMessage = "Ingested Changes")
    {
        throw new NotImplementedException();
        /*
        var byType = plan.Steps.ToLookup(t => t.GetType());
        var backupFiles = byType[typeof(IngestSteps.BackupFile)]
            .OfType<IngestSteps.BackupFile>()
            .Select(f => ((IStreamFactory)new NativeFileStreamFactory(f.Source), f.Hash, f.Size));
        await _archiveManager.BackupFiles(backupFiles);
        */

        //return _loadoutRegistry.Alter(plan.Loadout.LoadoutId, commitMessage, new IngestVisitor(byType, plan));
    }

    /*
    private class IngestVisitor : ALoadoutVisitor
    {
        private readonly IngestPlan _plan;
        private readonly HashSet<GamePath> _removeFiles;
        private readonly ILookup<ModId,FromArchive> _replaceFiles;
        private readonly ILookup<ModId,FromArchive> _createFiles;

        public IngestVisitor(ILookup<Type, IIngestStep> steps, IngestPlan plan)
        {
            _plan = plan;
            _removeFiles = steps[typeof(IngestSteps.RemoveFromLoadout)]
                .OfType<IngestSteps.RemoveFromLoadout>()
                .Select(r => plan.Loadout.Installation.ToGamePath(r.Source))
                .ToHashSet();

            _replaceFiles = steps[typeof(IngestSteps.ReplaceInLoadout)]
                .OfType<IngestSteps.ReplaceInLoadout>()
                .ToLookup(r => r.ModId,
                    r => new FromArchive
                    {
                        To = plan.Loadout.Installation.ToGamePath(r.Source),
                        Hash = r.Hash,
                        Size = r.Size,
                        Id = r.ModFileId
                    });

            _createFiles = steps[typeof(IngestSteps.CreateInLoadout)]
                .OfType<IngestSteps.CreateInLoadout>()
                .ToLookup(r => r.ModId,
                    r => new FromArchive
                    {
                        To = plan.Loadout.Installation.ToGamePath(r.Source),
                        Hash = r.Hash,
                        Size = r.Size,
                        Id = ModFileId.New()
                    });

        }

        public override AModFile? Alter(AModFile file)
        {
            if (file is IToFile to)
            {
                if (_removeFiles.Contains(to.To)) return null;
            }
            return base.Alter(file);
        }

        public override Mod? Alter(Mod mod)
        {
            var toAdd = _replaceFiles[mod.Id].Concat(_createFiles[mod.Id]);
            if (toAdd.Any())
            {
                mod = mod with
                {
                    Files = mod.Files.With(toAdd, f => f.Id)
                };
            }
            return base.Alter(mod);
        }
    }
    */
}

