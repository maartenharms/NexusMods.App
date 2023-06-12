using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.JsonConverters;
using NexusMods.Paths;

namespace NexusMods.Games.BethesdaGameStudios;

[JsonTypeId<PluginAnalysisData>("830565AC-E577-47A6-8762-A903A57004F9")]
public class PluginAnalysisData : IFileAnalysisData
{
    public required RelativePath[] Masters { get; init; }
    public bool IsLightMaster { get; init; }
}
