using NexusMods.DataModel.Games;

namespace NexusMods.App.UI.Controls.WorkspaceGrid.PaneDefinitions;

public interface IPaneDefinition
{
    /// <summary>
    /// Global unique identifier for this pane type. This is used to identify the pane type for instances
    /// in the workspace.
    /// </summary>
    public PaneContentId Id { get; }
    
    /// <summary>
    /// The name of the pane type. This is used for display purposes and never for identification (should be localized)
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Returns true if the given game installation is supported by this pane type.
    /// </summary>
    /// <param name="installation"></param>
    /// <returns></returns>
    public bool SupportsGame(GameInstallation installation);

    /// <summary>
    /// Returns true if the given context type is supported by this pane type. For example a loadout grid pane may
    /// take a LoadoutId as a context type. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool SupportsContext<T>();
    
    /// <summary>
    /// Creates a view model instance for this pane type.
    /// </summary>
    /// <returns></returns>
    public IPaneContentViewModel CreateViewModel(IPaneViewModel pane);
    
}
