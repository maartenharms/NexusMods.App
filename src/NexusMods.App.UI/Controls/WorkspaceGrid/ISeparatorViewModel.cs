namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public interface ISeparatorViewModel : IViewModelInterface
{
    public IPaneViewModel PaneA { get; }
    public IPaneViewModel PaneB { get; }
    public Direction Direction { get; }
    
    /// <summary>
    /// True if panes can be joined, in either direction
    /// </summary>
    public bool CanJoin { get; }

    public IWorkspaceGridViewModel Workspace { get; }
    
    public (PaneId, PaneId, Direction) Id => (PaneA.Id, PaneB.Id, Direction);
    
    /// <summary>
    /// Moves the separator by the given delta in screen pixels
    /// </summary>
    /// <param name="delta"></param>
    void Move(double delta);
    
    /// <summary>
    /// Split the A pane into two panes
    /// </summary>
    void SplitA();
    
    /// <summary>
    /// Split the B pane into two panes
    /// </summary>
    void SplitB();
    
    /// <summary>
    /// Swap the A and B panes
    /// </summary>
    void Swap();
    
    /// <summary>
    /// Deletes A and expands B to take its place
    /// </summary>
    void JoinAToB();
    
    /// <summary>
    /// Deletes B and expands A to take its place
    /// </summary>
    void JoinBToA();
}
