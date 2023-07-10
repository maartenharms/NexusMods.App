using Avalonia;
using Avalonia.Controls;
using FluentAssertions;
using NexusMods.App.UI.Controls.WorkspaceGrid;
using Noggog;

namespace NexusMods.UI.Tests.Controls.WorkspaceGrid;

public class WorkspaceGridViewModelTests : AVmTest<IWorkspaceGridViewModel>
{
    public WorkspaceGridViewModelTests(IServiceProvider provider) : base(provider)
    {
        
    }

    [Fact]
    public void CanSplitPanes()
    {
        Vm.Panes.Count.Should().Be(1);
        
        Vm.Split(Vm.Panes.First(), Direction.Vertical);
        Vm.Panes.Count.Should().Be(2);

        Vm.Panes.Select(p => p.LogicalBounds)
            .Should()
            .Contain(new Rect(0, 0, 0.5, 1))
            .And
            .Contain(new Rect(0.5, 0, 0.5, 1));

    }


}
