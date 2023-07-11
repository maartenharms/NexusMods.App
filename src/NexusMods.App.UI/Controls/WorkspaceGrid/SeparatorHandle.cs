namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public record struct SeparatorHandle
{
    public Direction Direction { get; init; }
    public Guid PaneA { get; init; }
    public Guid PaneB { get; init; }
    
    public IWorkspaceGridViewModel Workspace { get; init; }
}
