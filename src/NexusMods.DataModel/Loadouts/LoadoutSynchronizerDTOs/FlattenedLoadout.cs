using NexusMods.DataModel.Loadouts.Mods;
using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs;

/// <summary>
/// A flattened loadout of files, and the mods that they belong to.
/// </summary>
public class FlattenedLoadout
{
    /// <summary>
    /// The files in the loadout
    /// </summary>
    public IReadOnlyDictionary<GamePath, ModFilePair> Files { get; init; } = new Dictionary<GamePath, ModFilePair>();
    
    /// <summary>
    /// The mods in the loadout in sorted order
    /// </summary>
    public IEnumerable<Mod> Mods { get; init; } = Array.Empty<Mod>();
    
    
    /// <summary>
    /// Deconstructs the object into its parts
    /// </summary>
    /// <param name="files"></param>
    /// <param name="mods"></param>
    public void Deconstruct(out IReadOnlyDictionary<GamePath, ModFilePair> files, out IEnumerable<Mod> mods)
    {
        files = Files;
        mods = Mods;
    }
}
