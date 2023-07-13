namespace NexusMods.App.UI.Controls.WorkspaceGrid.PaneDefinitions;

public interface IPaneContentViewModel
{
    /// <summary>
    /// The pane definition that created this pane
    /// </summary>
    public IPaneDefinition Definition { get; }
    
    /// <summary>
    /// The title of the pane, that is displayed in the header
    /// </summary>
    public string Title { get; }
}
