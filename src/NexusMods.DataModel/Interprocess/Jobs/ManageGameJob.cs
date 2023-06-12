using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.Games;
using NexusMods.DataModel.JsonConverters;
using NexusMods.DataModel.Loadouts;

namespace NexusMods.DataModel.Interprocess.Jobs;

/// <summary>
/// A job that shows progress of the game management tasks
/// </summary>
[JsonTypeId<ManageGameJob>("A1474351-3EA9-4B86-A06E-EDC45354ECD7")]
public record ManageGameJob : AJobEntity, ILoadoutJob
{
    /// <inheritdoc />
    public LoadoutId LoadoutId { get; init; }
}
