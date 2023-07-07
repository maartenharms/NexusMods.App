using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class WorkspaceGridScratchSpace : UserControl
{
    public WorkspaceGridScratchSpace()
    {
        InitializeComponent();
        EditMode.IsChecked = true;
        WorkspaceControl.AddPane();
        WorkspaceControl.AddPane();
    }

    private void AddPanel(object? sender, RoutedEventArgs e)
    {
        WorkspaceControl.AddPane();
    }

    private void ChangeEditMode(object? sender, RoutedEventArgs e)
    {
        WorkspaceControl.EditMode = EditMode.IsChecked ?? false;
    }
}

