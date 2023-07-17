namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;

/// <summary>
/// Validation state for fingerprinting on IGeneratedFiles
/// </summary>
public record FingerprintingValidationState
{
    /// <summary>
    /// The Loadout the fingerprinting is being performed for.
    /// </summary>
    public required Loadout Loadout { get; init; }
    
    /// <summary>
    /// The flattend version of the Loadout
    /// </summary>
    public required FlattenedLoadout FlattenedLoadout { get; init; }
    
    

}
