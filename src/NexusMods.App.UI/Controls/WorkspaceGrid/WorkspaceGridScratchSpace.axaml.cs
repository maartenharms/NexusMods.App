using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Splat;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class WorkspaceGridScratchSpace : UserControl
{
    private readonly IWorkspaceGridViewModel? _vm;

    public WorkspaceGridScratchSpace()
    {
        InitializeComponent();
        EditMode.IsChecked = true;
        _vm = Locator.Current.GetService<IServiceProvider>()!.GetRequiredService<IWorkspaceGridViewModel>();
        WorkspaceGridHost.ViewModel = _vm;
    }

    private void AddPanel(object? sender, RoutedEventArgs e)
    {
        _vm?.Split();
    }

    private void ChangeEditMode(object? sender, RoutedEventArgs e)
    {
        if (_vm != null) 
            _vm.EditMode = EditMode.IsChecked ?? false;
    }
}

