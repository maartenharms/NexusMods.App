using DynamicData.Binding;
using NexusMods.App.UI.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class HandleViewModel : AViewModel<IHandleViewModel>, IHandleViewModel
{
    [Reactive]
    public Direction Direction { get; private set; }
    
    [Reactive]
    public IPaneViewModel PaneA { get; private set; }
    
    [Reactive ]
    public IPaneViewModel PaneB { get; private set; }

    public HandleViewModel(IPaneViewModel a, IPaneViewModel b, Direction direction, IWorkspaceGridViewModel workspace)
    {
        Direction = direction;
        PaneA = a;
        PaneB = b;

    }

    public void Swap()
    {
        var aLoc = PaneA.LogicalBounds;
        var bLoc = PaneB.LogicalBounds;
        // Swap the location of the two panes
        PaneA.SetLogicalBounds(bLoc);
        PaneB.SetLogicalBounds(aLoc);
        // Swap the panes in the handle so that they are still in the correct order (left->right, top->bottom)
        (PaneA, PaneB) = (PaneB, PaneA);
    }

    [Reactive] public bool CanCollapseAndRemoveA { get; private set; } = false;

    [Reactive] public bool CanCollapseAndRemoveB { get; private set; } = false;
}
