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
            pane1.Height.Should().Be(AvaloniaApp.DefaultHeight);
        });
        
        ViewModel.Split(ViewModel.Panes.First(), Direction.Vertical);
        await EventuallyOnUi(() =>
        {
            ctl.Children.Count.Should().Be(2);
        });
    }
}
