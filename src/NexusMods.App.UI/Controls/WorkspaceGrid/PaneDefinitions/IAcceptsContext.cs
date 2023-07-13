namespace NexusMods.App.UI.Controls.WorkspaceGrid.PaneDefinitions;

/// <summary>
/// Interface for setting the context of a pane. For example this may be used
/// to set the loadout ID for a loadout grid pane. Multiple context types may be
/// on a single pane.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAcceptsContext<in T>
{
    public void SetContext(T context);
}
