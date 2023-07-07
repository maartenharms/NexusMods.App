using System.Collections.ObjectModel;
using System.Reactive.Disposables;
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


    public WorkspaceGridViewModel(IServiceProvider provider)
    {
        var pane = provider.GetRequiredService<IPaneViewModel>();
        pane.SetLogicalBounds(new Rect(0, 0, 1, 1));
        _panes.AddOrUpdate(pane);
        this.WhenActivated(d =>
        {
            _panes.Connect()
                .Bind(out _panesFiltered)
                .Subscribe()
                .DisposeWith(d);
        });
    }
    
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
                break;
            }
            case Direction.Horizontal:
            {
                var newHeight = pane.LogicalBounds.Height / 2;
                var newPane = pane.Clone();
                
                newPane.SetLogicalBounds(newPane.LogicalBounds.WithY(pane.LogicalBounds.Top + newHeight).WithHeight(newHeight));
                pane.SetLogicalBounds(pane.LogicalBounds.WithHeight(newHeight));
                AddPane(newPane);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
}
