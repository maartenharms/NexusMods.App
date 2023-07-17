using FluentAssertions;
using NexusMods.DataModel.Extensions;
using NexusMods.DataModel.Loadouts.ApplySteps;
using NexusMods.DataModel.Loadouts.ModFiles;
using NexusMods.DataModel.Tests.Harness;
using NexusMods.DataModel.TriggerFilter;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;

namespace NexusMods.DataModel.Tests.LoadoutSynchronizerTests;

public class MakeApplyPlanTests : ALoadoutSynrchonizerTest<MakeApplyPlanTests>
{
    public MakeApplyPlanTests(IServiceProvider provider) : base(provider) { }

    
    #region Make Apply Plan Tests

    /// <summary>
    /// If a file doesn't exist, it should be created 
    /// </summary>
    [Fact]
    public async Task FilesThatDontExistAreCreatedByPlan()
    {
        var loadout = await CreateApplyPlanTestLoadout();
        
        var plan = await ValidateSuccess(loadout);

        var fileOne = loadout.Mods.Values.First().Files.Values.OfType<IFromArchive>()
            .First(f => f.Hash == Hash.From(0x00001));

        var absPath = loadout.Installation.Locations[GameFolderType.Game].Combine("0x00001.dat");

        plan.ToExtract.Should().Contain(KeyValuePair.Create(absPath, fileOne.Hash), "the file should be extracted");
    }
    
    /// <summary>
    /// Files that are already in the correct state in the game folder shouldn't be re-extracted
    /// </summary>
    [Fact]
    public async Task FilesThatExistAreNotCreatedByPlan()
    {
        var loadout = await CreateApplyPlanTestLoadout();
        
        var fileOne = loadout.Mods.Values.First().Files.Values.OfType<IFromArchive>()
            .First(f => f.Hash == Hash.From(0x00001));


        var absPath = loadout.Installation.Locations[GameFolderType.Game].Combine("0x00001.dat");

        TestIndexer.Entries.Add(new HashedEntry(absPath, fileOne.Hash, DateTime.Now - TimeSpan.FromDays(1), fileOne.Size ));
        
        var plan = await ValidateSuccess(loadout);
        
        plan.ToExtract.Should().NotContain(KeyValuePair.Create(absPath, fileOne.Hash), "the file is already extracted");
    }
    
    /// <summary>
    /// Files that are in the game folders, but not in the plan should be backed up then deleted
    /// </summary>
    [Fact]
    public async Task ExtraFilesAreDeletedAndBackedUp()
    { 
        var loadout = await CreateApplyPlanTestLoadout();
        
        var absPath = loadout.Installation.Locations[GameFolderType.Game].Combine("file_to_delete.dat");
        TestIndexer.Entries.Add(new HashedEntry(absPath, Hash.From(0x042), DateTime.Now - TimeSpan.FromDays(1), Size.From(0x33)));

        var plan = await ValidateSuccess(loadout);
        
        plan.ToDelete.Should().Contain(absPath, "the file should be deleted");
    }

    /// <summary>
    /// Generated files that have never been generated before should be generated
    /// </summary>
    [Fact]
    public async Task GeneratedFilesAreCreated()
    {
        var loadout = await CreateApplyPlanTestLoadout(generatedFile: true);

        var fileOne = loadout.Mods.Values.First().Files.Values.OfType<IGeneratedFile>()
            .First();

        var absPath = loadout.Installation.Locations[GameFolderType.Game].Combine("0x00001.generated");
        
        var plan = await ValidateSuccess(loadout);

        var generateFile = plan.ToGenerate.FirstOrDefault();
        generateFile.Should().NotBeNull();

        generateFile!.Value.GeneratedFile.Should().Be(fileOne);
        generateFile!.Key.Should().BeEquivalentTo(absPath);
        generateFile.Value.Fingerprint.Should().Be(Fingerprint.From(17241709254077376921));
    }

    /// <summary>
    /// If a generated file would create data that is already backed up and archived, then extract it instead
    /// of regenerating the file
    /// </summary>
    [Fact]
    public async Task GeneratedFilesFromArchivesAreExtracted()
    {
        var loadout = await CreateApplyPlanTestLoadout(generatedFile: true);

        TestGeneratedFileFingerprintCache.Dict.Add(Fingerprint.From(17241709254077376921), new CachedGeneratedFileData
        {
            Hash = Hash.From(0x42),
            Size = Size.From(0x43)
        });

        TestArchiveManagerInstance.Archives.Add(Hash.From(0x42));

        var absPath = loadout.Installation.Locations[GameFolderType.Game].Combine("0x00001.generated");
        
        var plan = await ValidateSuccess(loadout);

        plan.ToExtract.Should().Contain(KeyValuePair.Create(absPath, Hash.From(0x42)));
    }
    
    #endregion

}
