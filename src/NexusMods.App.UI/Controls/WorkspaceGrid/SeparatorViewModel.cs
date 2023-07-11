using NexusMods.App.UI.Extensions;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class SeparatorViewModel : AViewModel<ISeparatorViewModel>, ISeparatorViewModel
{
    public required IPaneViewModel PaneA { get; init; }
    public required IPaneViewModel PaneB { get; init; }
    public required Direction Direction { get; init; }
    
    public required IWorkspaceGridViewModel Workspace { get; init; }
    
    public void Move(double delta)
    {
        if (Direction == Direction.Vertical)
        {
            var logicalDelta = delta / (PaneA.ActualBounds.Width / PaneA.LogicalBounds.Width);
            var seam = PaneB.LogicalBounds.X;
            
            foreach (var pane in Workspace.Panes)
            {
                if (pane.LogicalBounds.X.EqualsWithTolerance(seam))
                {
                    pane.SetLogicalBounds(pane.LogicalBounds
                        .WithX(pane.LogicalBounds.X - logicalDelta)
                        .WithWidth(pane.LogicalBounds.Width + logicalDelta));
                }
                if (pane.LogicalBounds.Right.EqualsWithTolerance(seam))
                {
                    pane.SetLogicalBounds(pane.LogicalBounds
                        .WithWidth(pane.LogicalBounds.Width - logicalDelta));
                }
            }
        }
        else
        {
            var logicalDelta = delta / (PaneA.ActualBounds.Height / PaneA.LogicalBounds.Height);
            var seam = PaneB.LogicalBounds.Y;
            
            foreach (var pane in Workspace.Panes)
            {
                if (pane.LogicalBounds.Top.EqualsWithTolerance(seam))
                {
                    pane.SetLogicalBounds(pane.LogicalBounds
                        .WithY(pane.LogicalBounds.Y - logicalDelta)
                        .WithHeight(pane.LogicalBounds.Height + logicalDelta));
                }
                if (pane.LogicalBounds.Bottom.EqualsWithTolerance(seam))
                {
                    pane.SetLogicalBounds(pane.LogicalBounds
                        .WithHeight(pane.LogicalBounds.Height - logicalDelta));
                }
            }
        }
    }
}
