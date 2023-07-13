using Vogen;

namespace NexusMods.App.UI.Controls.WorkspaceGrid.PaneDefinitions;

/// <summary>
/// A unique identifier for a pane content type. Each tool or pane type should have a unique ID. This ID
/// is on the class level, not on the instance. 
/// </summary>
[ValueObject<Guid>]
public partial struct PaneContentId
{
    
}
