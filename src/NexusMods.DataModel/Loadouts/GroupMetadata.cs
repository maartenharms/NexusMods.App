using JetBrains.Annotations;
using NexusMods.DataModel.JsonConverters;
using NexusMods.DataModel.Loadouts.Mods;

namespace NexusMods.DataModel.Loadouts;

/// <summary>
/// Represents metadata that groups multiple <see cref="Mod"/> entities together.
/// </summary>
[PublicAPI]
[JsonTypeId("0050B428-134B-4C65-A3B7-EEC60694DD90", "GroupMetadata")]
public record GroupMetadata : AModMetadata
{
    /// <summary>
    /// Unique identifier of the group.
    /// </summary>
    public required GroupId Id { get; init; }

    /// <summary>
    /// Creation reason of the group.
    /// </summary>
    public GroupCreationReason CreationReason { get; set; } = GroupCreationReason.MultipleModsOneArchive;
}

/// <summary>
/// Represents the creation reason for a group
/// </summary>
[PublicAPI]
public enum GroupCreationReason : byte
{
    /// <summary>
    /// The group was created because an archive contained multiple mods.
    /// </summary>
    MultipleModsOneArchive = 0,
}
