using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.JsonConverters;

namespace NexusMods.Games.Generic.Entities;

[JsonTypeId<IniAnalysisData>("8CABD77A-C634-4659-8F0E-494938EBC252")]
public class IniAnalysisData : IFileAnalysisData
{
    public required HashSet<string> Sections { get; init; }
    public required HashSet<string> Keys { get; init; }
}
