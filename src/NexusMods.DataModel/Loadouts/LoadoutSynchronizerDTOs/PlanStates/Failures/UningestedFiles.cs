namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates.Failures;

/// <summary>
/// Error: The game folders contain files that have not been ingested yet.
/// </summary>
public record UningestedFiles : CompletedValidationState, IFailedValidation
{
    /// <inheritdoc />
    public string Message { get; init; } = "The game folders contain files that have not been ingested yet.";
    
    /// <summary>
    /// Conflicts that would occur if the loadout was applied. In other words, files that would be overwritten, or
    /// deleted that do not exist as part of a previous version of the loadout
    /// </summary>
    public IEnumerable<HashedEntry> Conflicts { get; init; } = Array.Empty<HashedEntry>();
}
