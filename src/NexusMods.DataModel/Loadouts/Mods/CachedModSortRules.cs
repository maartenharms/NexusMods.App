using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.JsonConverters;
using NexusMods.DataModel.Sorting.Rules;

namespace NexusMods.DataModel.Loadouts.Mods;

/// <summary>
/// Cached sort rules for a mod. 
/// </summary>
[JsonTypeId<CachedModSortRules>("7E3C43AD-BF4F-429C-8613-74BD53F24869")]
public record CachedModSortRules : Entity
{
    /// <summary>
    /// The cached sort rules
    /// </summary>
    public required ISortRule<Mod, ModId>[] Rules { get; init; }

    /// <inheritdoc />
    public override EntityCategory Category => EntityCategory.Fingerprints;
}
