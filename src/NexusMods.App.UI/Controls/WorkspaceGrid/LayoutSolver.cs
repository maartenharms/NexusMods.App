using Avalonia;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class LayoutSolver
{
    private List<PaneDefinition> _paneDefinitions;
    
    public IEnumerable<PaneDefinition> PaneDefinitions => _paneDefinitions;

    public LayoutSolver()
    {
        _paneDefinitions = new List<PaneDefinition>();
    }

    /// <summary>
    /// Updates the pane definitions and sets the actual values based on the relative values and the size of the workspace.
    /// </summary>
    /// <param name="size"></param>
    public void Solve(Size size)
    {
        foreach (var pane in _paneDefinitions)
        {
            pane.ActualLeft = pane.Left * size.Width;
            pane.ActualTop = pane.Top * size.Height;
            pane.ActualWidth = pane.Width * size.Width;
            pane.ActualHeight = pane.Height * size.Height;
        }
    }
    
    public void AddPane(PaneDefinition pane)
    {
        _paneDefinitions.Add(pane);
    }

    /// <summary>
    /// Splits the biggest pane in the direction with the most space.
    /// </summary>
    public void Split()
    {
        var pane = _paneDefinitions
            .OrderByDescending(x => x.Area)
            .ThenBy(x => x.Top)
            .ThenBy(x => x.Left)
            .First();
        
        Split(pane.Width > pane.Height ? Direction.Horizontal : Direction.Vertical, pane);
    }

    /// <summary>
    /// Split the given pane in the given direction, the new pane will be a clone of the given pane.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="pane"></param>
    public void Split(Direction direction, PaneDefinition pane)
    {
        switch (direction)
        {
            case Direction.Horizontal:
            {
                var newWidth = pane.Width / 2;
                var newPane = pane.Clone();
                newPane.Width = newWidth;
                newPane.Left = pane.Left + newWidth;
                pane.Width = newWidth;
                _paneDefinitions.Add(newPane);
                break;
            }
            case Direction.Vertical:
            {
                var newHeight = pane.Height / 2;
                var newPane = pane.Clone();
                newPane.Height = newHeight;
                newPane.Top = pane.Top + newHeight;
                pane.Height = newHeight;
                _paneDefinitions.Add(newPane);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
    
    /// <summary>
    /// Checks if the current configuration is valid, meaning no panes overlap.
    /// </summary>
    /// <returns></returns>
    public bool ValidConfiguration()
    {
        bool Overlapping(PaneDefinition a, PaneDefinition b)
        {
            var aRight = a.Left + a.Width;
            var bRight = b.Left + b.Width;
            var aBottom = a.Top + a.Height;
            var bBottom = b.Top + b.Height;
            
            return a.Left < bRight && aRight > b.Left && a.Top < bBottom && aBottom > b.Top;
        }
        
        
        for(var i = 0; i < _paneDefinitions.Count; i++)
        {
            for(var j = i + 1; j < _paneDefinitions.Count; j++)
            {
                if (Overlapping(_paneDefinitions[i], _paneDefinitions[j]))
                {
                    return false;
                }
            }
        }
        
        // Check if the panes cover the entire workspace
        return Math.Abs(_paneDefinitions.Sum(pane => pane.Area) - 1.0D) < Tolerance;
    }

    private const double Tolerance = 0.0000001f;
}
