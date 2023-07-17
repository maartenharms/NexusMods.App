using NexusMods.DataModel.Loadouts.ModFiles;
using NexusMods.DataModel.TriggerFilter;
using NexusMods.Hashing.xxHash64;

namespace NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs;

/// <summary>
/// A helper class to store the metadata of a file and it's fingerprint
/// </summary>
public class GeneratedFileState
{
    /// <summary>
    /// The IGeneratedFile instance
    /// </summary>
    public required IGeneratedFile GeneratedFile { get; init; }

    /// <summary>
    /// The file and the mod it belongs to
    /// </summary>
    public required ModFilePair ModFilePair { get; init; }
    
    /// <summary>
    /// The fingerprint of the file
    /// </summary>
    public required Fingerprint Fingerprint { get; init; }
}
