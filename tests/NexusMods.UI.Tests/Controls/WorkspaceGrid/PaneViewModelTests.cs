using Avalonia;
using FluentAssertions;
using NexusMods.App.UI.Controls.WorkspaceGrid;

namespace NexusMods.UI.Tests.Controls.WorkspaceGrid;

public class PaneViewModelTests : AVmTest<IPaneViewModel>
{
    public PaneViewModelTests(IServiceProvider provider) : base(provider) { }

    [Fact]
    public void SettingPaneSizesUpdatesProperties()
    {
        Vm.SetLogicalBounds(new Rect(0, 0, 1, 1));
        Vm.Arrange(new Size(1024, 768));
        
        Vm.LogicalBounds.Should().Be(new Rect(0, 0, 1, 1));
        Vm.ActualBounds.Should().Be(new Rect(0, 0, 1024, 768));

        Vm.IsAnchoredTop.Should().BeTrue();
        Vm.IsAnchoredLeft.Should().BeTrue();
        Vm.IsAnchoredRight.Should().BeTrue();
        Vm.IsAnchoredBottom.Should().BeTrue();
        
        Vm.SetLogicalBounds(new Rect(0.5, 0.5, 0.5, 0.5));
        
        Vm.ActualBounds.Should().Be(new Rect(512, 384, 512, 384));
        
        Vm.IsAnchoredTop.Should().BeFalse();
        Vm.IsAnchoredLeft.Should().BeFalse();
        Vm.IsAnchoredRight.Should().BeTrue();
        Vm.IsAnchoredBottom.Should().BeTrue();
        
        Vm.SetLogicalBounds(new Rect(0.5, 0.5, 0.25, 0.25));
        
        Vm.ActualBounds.Should().Be(new Rect(512, 384, 256, 192));
        
        Vm.IsAnchoredTop.Should().BeFalse();
        Vm.IsAnchoredLeft.Should().BeFalse();
        Vm.IsAnchoredRight.Should().BeFalse();
        Vm.IsAnchoredBottom.Should().BeFalse();
        
    }
}
