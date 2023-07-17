using System.Collections.ObjectModel;
using NexusMods.DataModel.Loadouts.ModFiles;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

/// <summary>
/// A successful validation result, with the files that would be created and deleted if the loadout was applied.
/// </summary>
public record SuccessfulValidationResult : CompletedValidationState
{
    /// <summary>
    /// Files that should be extracted from archives
    /// </summary>
    public required IReadOnlyDictionary<AbsolutePath, Hash> ToExtract { get; init; }
    
    /// <summary>
    /// Files that should be generated
    /// </summary>
    public required IReadOnlyDictionary<AbsolutePath, GeneratedFileState> ToGenerate { get; init; }
    
    /// <summary>
    /// Files that would be deleted if the loadout was applied.
    /// </summary>
    /// <returns></returns>
    public required IReadOnlySet<AbsolutePath> ToDelete { get; init; }
}
