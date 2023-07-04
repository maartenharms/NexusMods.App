using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class WorkspaceGridScratchSpace : UserControl
{
    public WorkspaceGridScratchSpace()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

