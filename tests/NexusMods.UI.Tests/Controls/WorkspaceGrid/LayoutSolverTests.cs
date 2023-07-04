using Avalonia;
using FluentAssertions;
using NexusMods.App.UI.Controls.WorkspaceGrid;

namespace NexusMods.UI.Tests.Controls.WorkspaceGrid;

public class LayoutSolverTests
{
    public LayoutSolverTests()
    {
    }
    
    [Fact]
    public void SplittingCreatesANewPanel()
    {
        var solver = new LayoutSolver();
        solver.AddPane(new PaneDefinition()
        {
            Width = 0.75,
            Height = 1.0
        });
        
        solver.Split();
        
        solver.PaneDefinitions.Count().Should().Be(2);

        foreach (var pane in solver.PaneDefinitions)
        {
            pane.Width.Should().Be(0.75);
            pane.Height.Should().Be(0.5);
        }
        
        solver.ValidConfiguration().Should().BeFalse("There's an empty space");
    }

    [Fact]
    public void SplittingByDefaultAlwaysSplitsTheBiggestPane()
    {
        var solver = new LayoutSolver();
        solver.AddPane(new PaneDefinition()
        {
            Width = 0.75,
            Height = 1.0
        });
        
        solver.AddPane(new PaneDefinition()
        {
            Width = 0.25,
            Height = 1.0,
            Left = 0.75
        });
        
        solver.Split();
        
        solver.PaneDefinitions.Count().Should().Be(3);
        solver.ValidConfiguration().Should().BeTrue();
        solver.PaneDefinitions.Should().ContainSingle(pane => Math.Abs(pane.Width - 0.25) < Tolerance);
    }

    [Fact]
    public void DefaultSplitDirectionIsVertical()
    {
        var solver = new LayoutSolver();
        solver.AddPane(new PaneDefinition
        {
            Width = 1.0,
            Height = 1.0
        });
        
        solver.Split();
        solver.PaneDefinitions.Count().Should().Be(2);
        solver.PaneDefinitions.Should()
            .AllSatisfy(x =>
            {
                x.Width.Should().Be(0.5);
                x.Height.Should().Be(1.0);
            });
    }

    [Fact]
    public void SettingActualSizesShouldUpdatePanes()
    {
        var solver = new LayoutSolver();
        solver.AddPane(new PaneDefinition
        {
            Width = 1.0,
            Height = 1.0
        });
        solver.Split();
        solver.Solve(new Size(1024, 768));

        solver.PaneDefinitions.Count().Should().Be(2);
        solver.PaneDefinitions.Should()
            .AllSatisfy(x =>
            {
                x.ActualWidth.Should().Be(512);
                x.ActualHeight.Should().Be(768);
            });
    }

    private const double Tolerance = 0.0001;
}
