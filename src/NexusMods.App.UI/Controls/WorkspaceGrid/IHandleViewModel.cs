namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public interface IHandleViewModel : IViewModelInterface
{
    public Direction Direction { get; }
    
    public IPaneViewModel PaneA { get; }
    public IPaneViewModel PaneB { get; }
    
    /// <summary>
    /// Swap the two panes attached to the separator
    /// </summary>
    public void Swap();
    
    /// <summary>
    /// True if the separator can be collapsed and the pane A removed
    /// </summary>
    public bool CanCollapseAndRemoveA { get; }
    
    /// <summary>
    /// True if the separator can be collapsed and the pane B removed
    /// </summary>
    public bool CanCollapseAndRemoveB { get; }

}
