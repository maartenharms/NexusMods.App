namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

/// <summary>
/// A failed validation of the loadout.
/// </summary>
public interface IFailedValidation
{
    /// <summary>
    /// Reason for the failure.
    /// </summary>
    public string Message { get; init; }
}
