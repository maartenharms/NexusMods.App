using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using NexusMods.App.UI.Extensions;
using ReactiveUI;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class WorkspaceGridView : ReactiveUserControl<IWorkspaceGridViewModel>
{
    private Size? _size;

    public WorkspaceGridView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            ViewModel!.Panes
                .ToObservableChangeSet()
                .OnUI()
                .Transform(CreatePaneView)
                .BindToUi(LayoutCanvas.Children)
                .DisposeWith(d);

            ViewModel!.Handles
                .ToObservableChangeSet()
                .OnUI()
                .Transform(CreateSeparatorView)
                .BindToUi(LayoutCanvas.Children)
                .DisposeWith(d);
                
                
            
            if (_size is not null)
                ViewModel.Arrange(_size.Value);
        });
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _size = finalSize;
        ViewModel?.Arrange(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return availableSize;
    }

    private Control CreatePaneView(IPaneViewModel paneViewModel)
    {
        var host = new PaneView
        {
            ViewModel = paneViewModel
        };
        return host;
    }

    private Control CreateSeparatorView(ISeparatorViewModel vm)
    {
        if (vm.Direction == Direction.Vertical)
        {
            return new HorizontalSeparatorView()
            {
                ViewModel = vm,
                ZIndex = 100
            };
        }
        else
        {
            return new VerticalSeparatorView()
            {
                ViewModel = vm,
                ZIndex = 100
            };
        }
    }
}

