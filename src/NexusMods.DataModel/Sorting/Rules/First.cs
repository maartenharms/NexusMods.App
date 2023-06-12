using NexusMods.DataModel.JsonConverters;

namespace NexusMods.DataModel.Sorting.Rules;

/// <summary />
[JsonTypeId("19213B5A-39D6-4F60-AF0D-C6AFBA64BB01", "First")]
public record First<TType, TId> : ISortRule<TType, TId>;
