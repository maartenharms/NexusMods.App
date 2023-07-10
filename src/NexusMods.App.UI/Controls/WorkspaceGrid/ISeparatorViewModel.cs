namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public interface ISeparatorViewModel : IViewModelInterface
{
    public IPaneViewModel PaneA { get; }
    public IPaneViewModel PaneB { get; }
    public Direction Direction { get; }

    public (PaneId, PaneId, Direction) Id => (PaneA.Id, PaneB.Id, Direction);
    
    /// <summary>
    /// Moves the separator by the given delta.
    /// </summary>
    /// <param name="delta"></param>
    void Move(double delta);
}
