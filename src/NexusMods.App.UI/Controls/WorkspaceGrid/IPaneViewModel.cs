using Avalonia;
using NexusMods.App.UI.Controls.WorkspaceGrid.PaneDefinitions;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public interface IPaneViewModel : IViewModelInterface
{
    public PaneId Id { get; }
    /// <summary>
    /// The position and size of the pane in ratio of the total size of the workspace, between 0 and 1
    /// </summary>
    Rect LogicalBounds { get; }
    
    /// <summary>
    /// The actual position and size of the pane in pixels, only available after a call to .Arrange
    /// </summary>
    Rect ActualBounds { get; }

    /// <summary>
    /// Set the ActualBounds of the pane based on the LogicalBounds and the total size of the workspace
    /// </summary>
    /// <param name="workspaceSize"></param>
    public void Arrange(Size workspaceSize);
    
    /// <summary>
    /// Sets the LogicalBounds of the pane, this is used to calculate the ActualBounds
    /// </summary>
    /// <param name="logicalBounds"></param>
    public void SetLogicalBounds(Rect logicalBounds);

    /// <summary>
    /// Returns true if the pane shares an edge with the given pane, but not just a corner
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool SharesEdgeWith(IPaneViewModel other);
    
    
    /// <summary>
    /// True if the pane is on the top edge of the workspace
    /// </summary>
    bool IsAnchoredTop { get; }
    
    /// <summary>
    /// Returns true if the pane is on the bottom edge of the workspace
    /// </summary>
    bool IsAnchoredBottom { get; }
    
    /// <summary>
    /// Returns true if the pane is on the left edge of the workspace
    /// </summary>
    bool IsAnchoredLeft { get; }
    
    /// <summary>
    /// Returns true if the pane is on the right edge of the workspace
    /// </summary>
    bool IsAnchoredRight { get; }

    /// <summary>
    /// Creates a new pane with the same content as this one
    /// </summary>
    /// <returns></returns>
    IPaneViewModel Clone();
    
    /// <summary>
    /// The content of the pane
    /// </summary>
    public IPaneContentViewModel? Content { get; set; }
}
