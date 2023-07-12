using System.Collections.ObjectModel;
using Avalonia;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public interface IWorkspaceGridViewModel : IViewModelInterface
{
    ReadOnlyObservableCollection<IPaneViewModel> Panes { get; }
    ReadOnlyObservableCollection<ISeparatorViewModel> Handles { get; }
    
    public bool EditMode { get; set; }
    
    void AddPane(IPaneViewModel pane);
    
    void Split(IPaneViewModel pane, Direction direction);

    /// <summary>
    /// Split the biggest pane in the direction with the most space. If panes are the same size, it will default
    /// to the one with the lowest top and left values (the pane in the top left corner).
    /// </summary>
    void Split();
    
    
    /// <summary>
    /// Informs the workspace that the given pane has been resized, and that the other panes should be resized to
    /// fill the space.
    /// </summary>
    /// <param name="size"></param>
    void Arrange(Size size);

    /// <summary>
    /// Issues update notifications for the given panes.
    /// </summary>
    /// <param name="panes"></param>
    void Refresh(params IPaneViewModel[] panes);

    /// <summary>
    /// Deletes paneB, and expands paneA to take its place
    /// </summary>
    /// <param name="paneA"></param>
    /// <param name="paneB"></param>
    void Join(IPaneViewModel paneA, IPaneViewModel paneB);
}
