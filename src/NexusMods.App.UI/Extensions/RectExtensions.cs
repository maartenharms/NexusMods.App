using Avalonia;

namespace NexusMods.App.UI.Extensions;

public static class RectExtensions
{
    /// <summary>
    /// Returns true if the two panes share a border.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool SharesBorderWith(this Rect a, Rect b)
    {
        var aRight = a.Left + a.Width;
        var bRight = b.Left + b.Width;
        var aBottom = a.Top + a.Height;
        var bBottom = b.Top + b.Height;
        
        // Check for a shared border
        if (aRight.EqualsWithTolerance(b.Left) || bRight.EqualsWithTolerance(a.Left))
        {
            // Check for overlapping borders
            if (LinesOverlap(a.Top, aBottom, b.Top, bBottom))
            {
                return true;
            }
        }
        
        // Check for horizontal borders
        if (aBottom.EqualsWithTolerance(b.Top) || bBottom.EqualsWithTolerance(a.Top))
        {
            // Check for vertical overlap
            if (LinesOverlap(a.Left, aRight, b.Left, bRight))
            {
                return true;
            }
        }

        return false;
    }

    private static bool LinesOverlap(double a1, double a2, double b1, double b2)
    {
        return Math.Min(a1, a2) < Math.Max(b1, b2) && Math.Max(a1, a2) > Math.Min(b1, b2);
    }
}
