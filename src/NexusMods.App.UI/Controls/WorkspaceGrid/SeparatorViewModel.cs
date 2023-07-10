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
            var logicalDelta = delta / (PaneA.ActualBounds.Width / PaneA.LogicalBounds.Width);
            Console.WriteLine("Moving vertical separator by {0} logical units", logicalDelta);
            PaneA.SetLogicalBounds(PaneA.LogicalBounds.WithWidth(PaneA.LogicalBounds.Width - logicalDelta));
            PaneB.SetLogicalBounds(PaneB.LogicalBounds
                .WithX(PaneB.LogicalBounds.X - logicalDelta)
                .WithWidth(PaneB.LogicalBounds.Width + logicalDelta));

        }
    }
}
