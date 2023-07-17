using System.Collections.ObjectModel;
using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

public record CompletedValidationState : PartiallyCompletedValidationState
{
    
    /// <summary>
    /// The state of the game folder.
    /// </summary>
    public required ReadOnlyDictionary<GamePath, HashedEntry> DiskState { get; init; }
}
