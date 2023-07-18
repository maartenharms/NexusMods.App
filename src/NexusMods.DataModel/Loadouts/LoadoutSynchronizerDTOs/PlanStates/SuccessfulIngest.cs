using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

/// <summary>
/// A successful ingest plan state.
/// </summary>
public record SuccessfulIngest : CompletedValidationState
{
    /// <summary>
    /// Files that need to be updated.
    /// </summary>
    public required IReadOnlyDictionary<GamePath, ModFilePair> ToUpdate { get; init; }
    
    /// <summary>
    /// Files that need to be added.
    /// </summary>
    public required IReadOnlyDictionary<GamePath, ModId> ToAdd { get; init; }
    
    /// <summary>
    /// Files that need to be deleted.
    /// </summary>
    public required IReadOnlySet<GamePath> ToDelete { get; init; }
    
    /// <summary>
    /// IGeneratedFile(s) that need to be parsed.
    /// </summary>
    public required IReadOnlyDictionary<AbsolutePath, ModFilePair> ToParse { get; init; }
    
    /// <summary>
    /// True if the plan is forking (should create a new loadout from the last applied loadout).
    /// </summary>
    public required bool IsForking { get; init; }
    
}
