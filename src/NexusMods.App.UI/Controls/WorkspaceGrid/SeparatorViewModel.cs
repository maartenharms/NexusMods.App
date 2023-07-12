using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using NexusMods.App.UI.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class SeparatorViewModel : AViewModel<ISeparatorViewModel>, ISeparatorViewModel
{
    public required IPaneViewModel PaneA { get; init; }
    public required IPaneViewModel PaneB { get; init; }
    public required Direction Direction { get; init; }
    
    [Reactive]
    public bool CanJoin { get; set; }
    public required IWorkspaceGridViewModel Workspace { get; init; }

    public SeparatorViewModel()
    {
        this.WhenActivated(d =>
        {
            var panes = Workspace!.Panes
                .ToObservableChangeSet()
                .QueryWhenChanged(panes => panes);
            if (Direction == Direction.Horizontal)
            {
                
                panes.CombineLatest(this.WhenAnyValue(vm => vm.PaneA.LogicalBounds),
                        this.WhenAnyValue(vm => vm.PaneB.LogicalBounds))
                    .Select(itms =>
                    {
                        var (panes, paneA, paneB) = itms;
                        var seam = paneB.Top;
                        var aBottomPanes = panes.Where(pane => pane.LogicalBounds.Top.EqualsWithTolerance(seam) && paneA.SharesBorderWith(pane.LogicalBounds));
                        var aTopPanes = panes.Where(pane => pane.LogicalBounds.Bottom.EqualsWithTolerance(seam) && paneB.SharesBorderWith(pane.LogicalBounds));
                        foreach (var tp in aTopPanes)
                        {
                            Console.WriteLine("Top Pane Size: {0} = {1}", tp.LogicalBounds, paneB);
                            
                        }
                        Console.WriteLine("Hor Split Count {0} {1}", aBottomPanes.Count(), aTopPanes.Count());
                        return  aBottomPanes.Count() == 1 && aTopPanes.Count() == 1;
                    })
                    .BindToUi(this, vm => vm.CanJoin)
                    .DisposeWith(d);
            }
            else
            {
                panes.CombineLatest(this.WhenAnyValue(vm => vm.PaneA.LogicalBounds),
                        this.WhenAnyValue(vm => vm.PaneB.LogicalBounds))
                    .Select(itms =>
                    {
                        var (panes, paneA, paneB) = itms;
                        var aRightPanes = panes.Where(pane => pane.LogicalBounds.Left.EqualsWithTolerance(paneA.Right) && paneA.SharesBorderWith(pane.LogicalBounds));
                        var bLeftPanes = panes.Where(pane => pane.LogicalBounds.Right.EqualsWithTolerance(paneB.Left) && paneB.SharesBorderWith(pane.LogicalBounds));
                        Console.WriteLine("Ver Split Count {0} {1}", aRightPanes.Count(), bLeftPanes.Count());
                        return  aRightPanes.Count() == 1 && bLeftPanes.Count() == 1;
                    })
                    .BindToUi(this, vm => vm.CanJoin)
                    .DisposeWith(d);
            }
        });
    }
    
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

    public void SplitA()
    {
        Workspace.Split(PaneA, Direction == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal);
    }

    public void SplitB()
    {
        Workspace.Split(PaneB, Direction == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal);
    }

    public void Swap()
    {
        var tmpLoc = PaneA.LogicalBounds;
        PaneA.SetLogicalBounds(PaneB.LogicalBounds);
        PaneB.SetLogicalBounds(tmpLoc);
        Workspace.Refresh(PaneA, PaneB);
    }

    public void JoinAToB()
    {
        if (!CanJoin) return;
        
        Workspace.Join(PaneA, PaneB);
    }

    public void JoinBToA()
    {
        if (!CanJoin) return;
        Workspace.Join(PaneB, PaneA);
    }
}
