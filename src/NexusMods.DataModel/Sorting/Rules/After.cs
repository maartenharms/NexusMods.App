using NexusMods.DataModel.JsonConverters;

namespace NexusMods.DataModel.Sorting.Rules;

/// <summary />
[JsonTypeId("CF834070-A5C5-484A-B865-EB5A8A209BB2", "After")]
public record After<TType, TId>(TId Other) : ISortRule<TType, TId>;
