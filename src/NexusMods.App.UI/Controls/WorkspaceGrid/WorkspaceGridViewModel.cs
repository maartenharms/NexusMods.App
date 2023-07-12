using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.Extensions;
using NexusMods.DataModel.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class WorkspaceGridViewModel : AViewModel<IWorkspaceGridViewModel>, IWorkspaceGridViewModel
{
    private readonly SourceCache<IPaneViewModel, PaneId> _panes = new(x => x.Id);

    private ReadOnlyObservableCollection<IPaneViewModel> _panesFiltered = Initializers.ReadOnlyObservableCollection<IPaneViewModel>();
    public ReadOnlyObservableCollection<IPaneViewModel> Panes => _panesFiltered;
    
    private ReadOnlyObservableCollection<ISeparatorViewModel> _handlesFiltered = Initializers.ReadOnlyObservableCollection<ISeparatorViewModel>();
    public ReadOnlyObservableCollection<ISeparatorViewModel> Handles => _handlesFiltered;
    
    [Reactive]
    public bool EditMode { get; set; }

    private static readonly PaneComparer PaneComparer = new();

    /// <summary>
    /// DI constructor.
    /// </summary>
    /// <param name="provider"></param>
    public WorkspaceGridViewModel(IServiceProvider provider)
    {
        var pane = provider.GetRequiredService<IPaneViewModel>();
        pane.SetLogicalBounds(new Rect(0, 0, 1, 1));
        _panes.AddOrUpdate(pane);
        this.WhenActivated(d =>
        {
            _panes.Connect()
                .Sort(PaneComparer)
                .Bind(out _panesFiltered)
                .Subscribe()
                .DisposeWith(d);

            _panes.Connect()
                .QueryWhenChanged(panes => GetSeparatorHandles(panes.Items))
                .ToDiffedChangeSet(pane => pane, 
                    pane => (ISeparatorViewModel)new SeparatorViewModel
                {
                    PaneA = pane.A,
                    PaneB = pane.B,
                    Direction = pane.Direction,
                    Workspace = this
                })
                .OnUI()
                .Bind(out _handlesFiltered)
                .Subscribe()
                .DisposeWith(d);
        });
    }
    
    /// <summary>
    /// Adds the given pane to the grid.
    /// </summary>
    /// <param name="pane"></param>
    public void AddPane(IPaneViewModel pane)
    {
        _panes.Edit(x =>
        {
            x.AddOrUpdate(pane);
        });
    }
    

    /// <summary>
    /// Splits the given pane in the given direction. Direction here is a bit confusing, as it's the direction of the
    /// seam being generated. So a Vertical split will generate a pane on the left and right, and a Horizontal split
    /// will generate a pane on the top and bottom.
    /// </summary>
    /// <param name="pane"></param>
    /// <param name="direction"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Split(IPaneViewModel pane, Direction direction)
    {
        switch (direction)
        {
            case Direction.Vertical:
            {
                var newWidth = pane.LogicalBounds.Width / 2;
                var newPane = pane.Clone();
                
                newPane.SetLogicalBounds(newPane.LogicalBounds.WithX(pane.LogicalBounds.Left + newWidth).WithWidth(newWidth));
                pane.SetLogicalBounds(pane.LogicalBounds.WithWidth(newWidth));
                AddPane(newPane);
                UpdatePane(pane);
                break;
            }
            case Direction.Horizontal:
            {
                var newHeight = pane.LogicalBounds.Height / 2;
                var newPane = pane.Clone();
                
                newPane.SetLogicalBounds(newPane.LogicalBounds.WithY(pane.LogicalBounds.Top + newHeight).WithHeight(newHeight));
                pane.SetLogicalBounds(pane.LogicalBounds.WithHeight(newHeight));
                AddPane(newPane);
                UpdatePane(pane);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
    
    public void Split()
    {
        var pane = Panes
            .OrderByDescending(x => x.ActualBounds.Width * x.ActualBounds.Height)
            .ThenBy(x => x.LogicalBounds.Top)
            .ThenBy(x => x.LogicalBounds.Left)
            .First();
        
        Split(pane, pane.ActualBounds.Height >= pane.ActualBounds.Width ? Direction.Horizontal : Direction.Vertical);
    }

    public void Arrange(Size size)
    {
        foreach (var pane in _panes.Items)
        {
            pane.Arrange(size);
        }
    }

    public void Refresh(params IPaneViewModel[] panes)
    {
        _panes.Edit(x =>
        {
            x.AddOrUpdate(panes);
        });
    }

    public void Join(IPaneViewModel paneA, IPaneViewModel paneB)
    {
        if (paneA.LogicalBounds.Top.EqualsWithTolerance(paneB.LogicalBounds.Top))
        {
            var newWidth = paneA.LogicalBounds.Width + paneB.LogicalBounds.Width;
            paneA.SetLogicalBounds(paneA.LogicalBounds.WithWidth(newWidth)
                .WithX(Math.Min(paneA.LogicalBounds.X, paneB.LogicalBounds.X)));
            _panes.Remove(paneB);
            UpdatePane(paneA);
        }
        else if (paneA.LogicalBounds.Left.EqualsWithTolerance(paneB.LogicalBounds.Left))
        {
            var newHeight = paneA.LogicalBounds.Height + paneB.LogicalBounds.Height;
            paneA.SetLogicalBounds(paneA.LogicalBounds.WithHeight(newHeight)
                .WithY(Math.Min(paneA.LogicalBounds.Y, paneA.LogicalBounds.Y)));
            _panes.Remove(paneB);
            UpdatePane(paneA);
        }
    }

    private void UpdatePane(IPaneViewModel pane)
    {
        _panes.Edit(x =>
        {
            x.AddOrUpdate(pane);
        });
    }
    
    public IEnumerable<(IPaneViewModel A, IPaneViewModel B, Direction Direction)> 
        GetSeparatorHandles(IEnumerable<IPaneViewModel> panesItems)
    {
        var panes = panesItems.ToList();
        for (var i = 0; i < panes.Count; i++)
        {
            for (var j = i + 1; j < panes.Count(); j++)
            {

                var paneA = panes[i];
                var paneB = panes[j];

                // Swap so that paneA is always to the left of paneB
                if (paneA.LogicalBounds.Left > paneB.LogicalBounds.Left)
                    (paneA, paneB) = (paneB, paneA);

                // Ignore cases where the panes are not adjacent

                if (!paneA.LogicalBounds.SharesBorderWith(paneB.LogicalBounds))
                    continue;


                if ((paneA.LogicalBounds.Left + paneA.LogicalBounds.Width).EqualsWithTolerance(paneB.LogicalBounds.Left))
                {
                    yield return (paneA, paneB, Direction.Vertical);
                }
            }
        }

        // Vertical separators
        for (var i = 0; i < panes.Count; i++)
        {
            for (var j = i + 1; j < panes.Count; j++)
            {
                var paneA = panes[i];
                var paneB = panes[j];

                // Swap so that paneA is always to the left of paneB
                if (paneA.LogicalBounds.Top > paneB.LogicalBounds.Top)
                    (paneA, paneB) = (paneB, paneA);
                
                if (!paneA.LogicalBounds.SharesBorderWith(paneB.LogicalBounds))
                    continue;

                if ((paneA.LogicalBounds.Top + paneA.LogicalBounds.Height).EqualsWithTolerance(paneB.LogicalBounds.Top))
                {
                    yield return (paneA, paneB, Direction.Horizontal);
                }
            }
        }
    }
    
  
}
