
using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.JsonConverters;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.ModFiles;

/// <summary>
/// Cached metadata for a generated file.
/// </summary>
[JsonTypeId<CachedGeneratedFileData>("4C8D84E6-6476-45AB-8F13-0434A1CF7CCC")]
public record CachedGeneratedFileData : Entity
{
    /// <inheritdoc />
    public override EntityCategory Category => EntityCategory.Fingerprints;
    
    /// <summary>
    /// The hash of the generated data
    /// </summary>
    public required Hash Hash { get; init; }
    
    /// <summary>
    /// The size of the generated data
    /// </summary>
    public required Size Size { get; init; }
}
