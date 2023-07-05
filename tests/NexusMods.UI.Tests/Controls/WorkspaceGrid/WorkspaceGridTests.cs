using Avalonia.VisualTree;
using FluentAssertions;
using NexusMods.App.UI.Controls.WorkspaceGrid;
using Noggog;

namespace NexusMods.UI.Tests.Controls.WorkspaceGrid;

public class WorkspaceGridTests : AViewTest<App.UI.Controls.WorkspaceGrid.WorkspaceGrid>
{
    public WorkspaceGridTests(IServiceProvider provider) : base(provider) { }

    [Fact]
    public void WorkspaceGridCreatesDefaultPanes()
    {
        View.Solver.PaneDefinitions.Should().HaveCount(1);
        View.GetVisualDescendants().Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CanAddPaneToWorkspaceGrid()
    {
        await OnUi(() =>
        {
            View.AddPane();
            View.Solver.PaneDefinitions.Should().HaveCount(2);
            View.GetVisualDescendants().Should().HaveCount(4);
        });
    }

    [Fact]
    public async Task SplittingCreatesPanels()
    {
        await OnUi(() =>
        {
            View.AddPane();
            View.Solver.PaneDefinitions.Should().HaveCount(2);
            View.GetVisualDescendants().Should().HaveCount(4);
            View.Handles.Should().HaveCount(1);
            View.Handles.First().Direction.Should().Be(Direction.Horizontal);
            
            View.AddPane();
            View.Solver.PaneDefinitions.Should().HaveCount(3);
            View.GetVisualDescendants().Should().HaveCount(9);
            View.Handles.Should().HaveCount(3);
            View.Handles.Skip(1).First().Direction.Should().Be(Direction.Horizontal);
        });
        
    }
}
