namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates.Failures;

/// <summary>
/// An apply plan cannot be created because there is no previously applied state, thus the loadout cannot be applied,
/// and Ingest must be run first.
/// </summary>
public record NoPreviouslyAppliedState : AValidationResult, IFailedValidation
{
    public string Message { get; init; } = "No previously applied state found, run ingest first.";
}
