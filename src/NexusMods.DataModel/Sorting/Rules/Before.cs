using NexusMods.DataModel.JsonConverters;

namespace NexusMods.DataModel.Sorting.Rules;

/// <summary />
[JsonTypeId("47FACF90-CC1A-4550-849E-B263B7063CF3", "Before")] 
public record Before<TType, TId>(TId Other) : ISortRule<TType, TId>;
