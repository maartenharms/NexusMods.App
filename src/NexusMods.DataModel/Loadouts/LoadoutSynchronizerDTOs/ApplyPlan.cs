using NexusMods.DataModel.Loadouts.ApplySteps;
using NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs;

/// <summary>
/// All the state data required to apply a loadout.
/// </summary>
public record ApplyPlan : ConflictState
{
    /// <summary>
    /// The steps required to apply the loadout.
    /// </summary>
    public required IEnumerable<IApplyStep> Steps { get; init; }
}
