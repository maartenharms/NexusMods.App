using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class WorkspaceGridViewModel : AViewModel<IWorkspaceGridViewModel>, IWorkspaceGridViewModel
{
    private readonly SourceCache<IPaneViewModel, PaneId> _panes = new(x => x.Id);

    private ReadOnlyObservableCollection<IPaneViewModel> _panesFiltered = Initializers.ReadOnlyObservableCollection<IPaneViewModel>();
    public ReadOnlyObservableCollection<IPaneViewModel> Panes => _panesFiltered;
    
    private readonly SourceCache<Separator, Separator> _handles = new(x => x);
    
    private ReadOnlyObservableCollection<Separator> _handlesFiltered = Initializers.ReadOnlyObservableCollection<Separator>();
    public ReadOnlyObservableCollection<Separator> Handles => _handlesFiltered;


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
            
            _handles.Connect()
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
                AddHandle(pane, newPane, direction);
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

    private void AddHandle(IPaneViewModel a, IPaneViewModel b, Direction direction)
    {
        _handles.AddOrUpdate(new Separator(a, b, direction));
    }

    public void Arrange(Size size)
    {
        foreach (var pane in _panes.Items)
        {
            pane.Arrange(size);
        }
    }

    private void UpdatePane(IPaneViewModel pane)
    {
        _panes.Edit(x =>
        {
            x.AddOrUpdate(pane);
        });
    }
}
