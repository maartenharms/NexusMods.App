using System.Reactive.Subjects;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class PaneViewModel : AViewModel<IPaneViewModel>, IPaneViewModel
{
    public PaneId Id { get; init; } = PaneId.From(Guid.NewGuid());
    
    private Size _workspaceSize = new(1, 1);
    
    private readonly IServiceProvider _provider;

    [Reactive]
    public Rect LogicalBounds { get; private set; }
    
    [Reactive]
    public Rect ActualBounds { get; private set; }

    public PaneViewModel(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    
    public void Arrange(Size workspaceSize)
    {
        _workspaceSize = workspaceSize;
        ActualBounds = new Rect(LogicalBounds.X * workspaceSize.Width, 
            LogicalBounds.Y * workspaceSize.Height,
            LogicalBounds.Width * workspaceSize.Width,
            LogicalBounds.Height * workspaceSize.Height);
    }

    public void SetLogicalBounds(Rect logicalBounds)
    {
        LogicalBounds = logicalBounds;
        Arrange(_workspaceSize);
        
        IsAnchoredLeft = LogicalBounds.X.EqualsWithTolerance(0.0);
        IsAnchoredTop = LogicalBounds.Top.EqualsWithTolerance(0.0);
        IsAnchoredRight = LogicalBounds.Right.EqualsWithTolerance(1.0);
        IsAnchoredBottom = LogicalBounds.Bottom.EqualsWithTolerance(1.0);
    }

    public bool SharesEdgeWith(IPaneViewModel other)
    {
        var a = this.LogicalBounds;
        var b = other.LogicalBounds;
        var aRight = a.Left + a.Width;
        var bRight = b.Left + b.Width;
        var aBottom = a.Top + a.Height;
        var bBottom = b.Top + b.Height;
        
        // Check for a shared border
        if (aRight.EqualsWithTolerance(b.Left) || b.Right.EqualsWithTolerance(a.Left))
        {
            // Check for overlapping borders
            if (LinesOverlap(a.Top, aBottom, b.Top, bBottom))
            {
                return true;
            }
        }
        
        // Check for horizontal borders
        if (aBottom.EqualsWithTolerance(b.Top) || b.Bottom.EqualsWithTolerance(a.Top))
        {
            // Check for vertical overlap
            if (LinesOverlap(a.Left, aRight, b.Left, bRight))
            {
                return true;
            }
        }

        return false;
    }
    
    private bool LinesOverlap(double a1, double a2, double b1, double b2)
    {
        return Math.Min(a1, a2) < Math.Max(b1, b2) && Math.Max(a1, a2) > Math.Min(b1, b2);
    }

    [Reactive]
    public bool IsAnchoredTop { get; private set; }
    
    [Reactive]
    public bool IsAnchoredBottom { get; private set; }
    
    [Reactive]
    public bool IsAnchoredLeft { get; private set; }
    
    [Reactive]
    public bool IsAnchoredRight { get; private set; }

    public IPaneViewModel Clone()
    {
        var pane = _provider.GetRequiredService<IPaneViewModel>();
        pane.SetLogicalBounds(LogicalBounds);
        pane.Arrange(_workspaceSize);
        return pane;
    }
}
