using System.Collections.ObjectModel;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public interface IWorkspaceGridViewModel : IViewModelInterface
{
    ReadOnlyObservableCollection<IPaneViewModel> Panes { get; }
    
    void AddPane(IPaneViewModel pane);
    
    void Split(IPaneViewModel pane, Direction direction);
}
