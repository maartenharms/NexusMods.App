using System.Collections.ObjectModel;
using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

/// <summary>
/// All the state data of a completed validation, validation errors will subclass this.
/// </summary>
public record PartiallyCompletedValidationState  : AValidationResult
{
    /// <summary>
    /// The Loadout that is being planned to be applied, but flattened for easier comparison.
    /// </summary>
    public required FlattenedLoadout FlattenedPlannedState { get; init; }
    
    /// <summary>
    /// The Loadout that is currently applied, or null if no loadout is applied (which is an error state)
    /// </summary>
    public required Loadout AppliedState { get; init; }
    
    /// <summary>
    /// The Loadout that is currently applied, or null if no loadout is applied (which is an error state), but flattened for easier comparison.
    /// </summary>
    public required FlattenedLoadout FlattenedAppliedState { get; init; }
}
