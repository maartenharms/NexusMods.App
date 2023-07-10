namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class SeparatorViewModel : AViewModel<ISeparatorViewModel>, ISeparatorViewModel
{
    public required IPaneViewModel PaneA { get; init; }
    public required IPaneViewModel PaneB { get; init; }
    public required Direction Direction { get; init; }
    
    public void Move(double delta)
    {
        if (Direction == Direction.Vertical)
        {
            PaneA.SetLogicalBounds(PaneA.LogicalBounds.WithWidth(PaneA.LogicalBounds.Width + delta));
            PaneB.SetLogicalBounds(PaneB.LogicalBounds
                .WithX(PaneB.LogicalBounds.X + delta)
                .WithWidth(PaneB.LogicalBounds.Width - delta));

        }
    }
}
