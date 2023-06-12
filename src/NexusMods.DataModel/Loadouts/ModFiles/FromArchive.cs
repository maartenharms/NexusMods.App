using NexusMods.DataModel.JsonConverters;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.ModFiles;

/// <summary>
/// Denotes any file which is originally sourced from an archive for installation.
/// </summary>
[JsonTypeId<FromArchive>("22C0313F-D0B8-4BE7-8DF1-45809F7483D8")]
public record FromArchive : AModFile, IFromArchive, IToFile
{
    /// <inheritdoc />
    public required Size Size { get; init; }

    /// <inheritdoc />
    public required Hash Hash { get; init; }

    /// <inheritdoc />
    public required GamePath To { get; init; }
}
