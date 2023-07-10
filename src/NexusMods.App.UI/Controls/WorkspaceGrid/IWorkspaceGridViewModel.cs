using System.Collections.ObjectModel;
using Avalonia;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public interface IWorkspaceGridViewModel : IViewModelInterface
{
    ReadOnlyObservableCollection<IPaneViewModel> Panes { get; }
    ReadOnlyObservableCollection<Separator> Handles { get; }
    
    void AddPane(IPaneViewModel pane);
    
    void Split(IPaneViewModel pane, Direction direction);

    /// <summary>
    /// Split the biggest pane in the direction with the most space. If panes are the same size, it will default
    /// to the one with the lowest top and left values (the pane in the top left corner).
    /// </summary>
    void Split()
    {
        var pane = Panes
            .OrderByDescending(x => x.ActualBounds.Width * x.ActualBounds.Height)
            .ThenBy(x => x.LogicalBounds.Top)
            .ThenBy(x => x.LogicalBounds.Left)
            .First();
        
        Split(pane, pane.ActualBounds.Height >= pane.ActualBounds.Width ? Direction.Horizontal : Direction.Vertical);
    }
    
    
    /// <summary>
    /// Informs the workspace that the given pane has been resized, and that the other panes should be resized to
    /// fill the space.
    /// </summary>
    /// <param name="size"></param>
    void Arrange(Size size);
}
