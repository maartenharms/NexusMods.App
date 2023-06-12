using NexusMods.DataModel.JsonConverters;
using NexusMods.DataModel.Loadouts;
using NexusMods.Paths;

namespace NexusMods.Games.BethesdaGameStudios;

[JsonTypeId<AnalysisSortData>("EDAEAC15-7D12-4417-A740-5A7BFFAE0DDE")]
public class AnalysisSortData : IModFileMetadata
{
    public required RelativePath[] Masters { get; init; } = Array.Empty<RelativePath>();
}
