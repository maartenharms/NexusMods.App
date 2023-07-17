namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

/// <summary>
/// The base configuration for an apply or ingest plan. 
/// </summary>
public record BaseConfiguration
{
    /// <summary>
    /// The planned state of thBe loadout.
    /// </summary>
    public required Loadout PlannedState { get; init; }
}