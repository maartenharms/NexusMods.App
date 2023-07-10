using Avalonia;
using Avalonia.Controls;
using FluentAssertions;
using NexusMods.App.UI.Controls.WorkspaceGrid;
using NexusMods.UI.Tests.Framework;
using Noggog;

namespace NexusMods.UI.Tests.Controls.WorkspaceGrid;

public class WorkspaceGridViewTests : AViewTest<WorkspaceGridView, WorkspaceGridViewModel, IWorkspaceGridViewModel>
{
    public WorkspaceGridViewTests(IServiceProvider provider) : base(provider, true) { }
    
    [Fact]
    public async Task WorkspaceGridCreatesDefaultPanes()
    {
        var ctl = await GetControl<Canvas>("LayoutCanvas");
        ctl.Children.Count.Should().Be(1);

        await EventuallyOnUi(() =>
        {
            var pane1 = (PaneView)ctl.Children.First();
            pane1.Width.Should().Be(AvaloniaApp.DefaultWidth);
            pane1.Height.Should().Be(AvaloniaApp.DefaultHeight - 48);
        });
        
        ViewModel.Split(ViewModel.Panes.First(), Direction.Vertical);
        await EventuallyOnUi(() =>
        {
            ctl.Children.Count.Should().Be(2);
        });
    }
    
        
    [Fact]
    public async Task MovingAHandleChangesThePaneSizes()
    {
        var ctl = await GetControl<Canvas>("LayoutCanvas");
        ViewModel.Split();
        
        ViewModel.Panes.Count.Should().Be(2);
        ViewModel.Handles.Count.Should().Be(1);

        await EventuallyOnUi(() =>
        {
            ctl.Children.Count.Should().Be(3);
        });
        
        ViewModel.Panes.First().LogicalBounds.Should().Be(new Rect(0, 0, 0.5, 1));
        ViewModel.Panes.Last().LogicalBounds.Should().Be(new Rect(0.5, 0, 0.5, 1));

        await OnUi(() =>
        {
            var handle = ViewModel.Handles.First();
            handle.Move(-0.25);
        });
        
        ViewModel.Panes.First().LogicalBounds.Should().Be(new Rect(0, 0, 0.25, 1));
        ViewModel.Panes.Last().LogicalBounds.Should().Be(new Rect(0.25, 0, 0.75, 1));

    }


}
