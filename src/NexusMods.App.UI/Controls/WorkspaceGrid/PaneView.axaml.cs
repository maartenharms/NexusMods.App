using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class PaneView : ReactiveUserControl<IPaneViewModel>
{
    public PaneView()
    {
        InitializeComponent();
        
        this.WhenActivated(d =>
        {
            var bounds = this.WhenAnyValue(view => view.ViewModel!.ActualBounds);
                
            bounds.Select(b => b.Width)
                .BindToUi(this, view => view.Width)
                .DisposeWith(d);
            
            bounds.Select(b => b.Height)
                .BindToUi(this, view => view.Height)
                .DisposeWith(d);
            
            bounds.Select(b => b.X)
                .OnUI()
                .Subscribe(val => Canvas.SetLeft(this, val))
                .DisposeWith(d);
            
            bounds.Select(b => b.Y)
                .OnUI()
                .Subscribe(val => Canvas.SetTop(this, val))
                .DisposeWith(d);

            this.WhenAnyValue(view => view.ViewModel!.Id)
                .BindToUi(this, view => view.HeaderTextBlock.Text)
                .DisposeWith(d);
            
        });
    }
}

