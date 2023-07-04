﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using DynamicData;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class WorkspaceGrid : Panel
{
    private readonly Dictionary<Guid,Control> _children;
    private readonly Dictionary<SeparatorHandle, Control> _handles;

    public WorkspaceGrid()
    {
        Solver = new LayoutSolver();
        Solver.AddPane(new PaneDefinition()
        {
            Id = Guid.NewGuid(),
            Width = 1.0,
            Height = 1.0
        });

        _children = new ();
        _handles = new ();
        SyncChildren();
    }

    public LayoutSolver Solver { get; set; }
    
    public IEnumerable<SeparatorHandle> Handles => _handles.Keys;

    public void AddPane()
    {
        Solver.Split();
        SyncChildren();
        SyncHandles();
        InvalidateArrange();
    }

    private void SyncHandles()
    {
        var seen = new HashSet<SeparatorHandle>();
        foreach (var handle in Solver.GetSeparatorHandles())
        {
            if (_handles.ContainsKey(handle))
            {
                seen.Add(handle);
            }
            else
            {
                seen.Add(handle);
                var visual = MakeHandle(handle.Direction);
                VisualChildren.Add(visual);
                _handles.Add(handle, visual);
            }
        }
        
        foreach (var (key, visual) in _handles)
        {
            if (seen.Contains(key))
                continue;

            _handles.Remove(key);
            VisualChildren.Remove(visual);
        }
    }

    private Control MakeHandle(Direction direction)
    {
        return direction switch
        {
            Direction.Horizontal => new Rectangle { Fill = Brushes.DarkGray, Width = 8, Height = 32, ZIndex = 1 },
            Direction.Vertical => new Rectangle { Fill = Brushes.LightGray, Width = 32, Height = 8, ZIndex = 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        SyncChildren();
        SyncHandles();
        Solver.Solve(finalSize);
        foreach (var pane in Solver.PaneDefinitions)
        {
            var visual = _children[pane.Id];
            visual.Arrange(new Rect(pane.ActualLeft, pane.ActualTop, pane.ActualWidth, pane.ActualHeight));
        }
        
        foreach (var (handle, visual) in _handles)
        {
            if (handle.Direction == Direction.Horizontal)
            {
                var sideA = Solver.PaneDefinitions.First(x => x.Id == handle.PaneA);
                var sideB = Solver.PaneDefinitions.First(x => x.Id == handle.PaneB);

                var biggestTop = Math.Max(sideA.ActualTop, sideB.ActualTop);
                var smallestBottom = Math.Min(sideA.ActualTop + sideA.ActualHeight,
                    sideB.ActualTop + sideB.ActualHeight);

                var top = biggestTop + ((smallestBottom - biggestTop) / 2) - (visual.Height / 2);

                visual.Arrange(new Rect(sideB.ActualLeft - (visual.Width / 2), top, visual.Width, visual.Height));
            }
            else
            {
                var sideA = Solver.PaneDefinitions.First(x => x.Id == handle.PaneA);
                var sideB = Solver.PaneDefinitions.First(x => x.Id == handle.PaneB);

                var biggestLeft = Math.Max(sideA.ActualLeft, sideB.ActualLeft);
                var smallestRight = Math.Min(sideA.ActualLeft + sideA.ActualWidth,
                    sideB.ActualLeft + sideB.ActualWidth);

                var left = biggestLeft + ((smallestRight - biggestLeft) / 2) - (visual.Width / 2);

                visual.Arrange(new Rect(left, sideB.ActualTop - (visual.Height / 2), visual.Width, visual.Height));
            }
        }
        
        return finalSize;
    }

    private void SyncChildren()
    {
        foreach (var child in _children)
        {
            if (Solver.PaneDefinitions.Any(pane => pane.Id == child.Key))
            {
                continue;
            }
            
            _children.Remove(child.Key);
            VisualChildren.Remove(child.Value);
        }

        foreach (var pane in Solver.PaneDefinitions)
        {
            if (_children.ContainsKey(pane.Id))
                continue;

            var newVisual = new Rectangle
            {
                Fill = SolidColorBrush.Parse("#"+pane.Id.ToString()[..6])
            };
            _children.Add(pane.Id, newVisual);
            VisualChildren.Add(newVisual);
        }
    }
}