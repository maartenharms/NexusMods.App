using Avalonia;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class PaneComparer : IComparer<IPaneViewModel>
{
    public int Compare(IPaneViewModel? x, IPaneViewModel? y)
    {
        var xComparison = x!.LogicalBounds.X.CompareTo(y!.LogicalBounds.X);
        if (xComparison != 0) return xComparison;
        return x.LogicalBounds.Y.CompareTo(y.LogicalBounds.Y);
    }
}
