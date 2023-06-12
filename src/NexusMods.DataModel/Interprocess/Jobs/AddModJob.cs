using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.JsonConverters;
using NexusMods.DataModel.Loadouts;

namespace NexusMods.DataModel.Interprocess.Jobs;

/// <summary>
/// A job that adds a mod to a loadout
/// </summary>
[JsonTypeId<AddModJob>("EC2DA75C-6D8E-47E7-B5A1-44769C93A5C4")]
public record AddModJob : AJobEntity, IModJob
{
    /// <inheritdoc />
    public LoadoutId LoadoutId { get; init; }

    /// <inheritdoc />
    public ModId ModId { get; init; }
}
