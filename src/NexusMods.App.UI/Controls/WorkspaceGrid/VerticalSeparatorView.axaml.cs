using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class VerticalSeparatorView : ReactiveUserControl<ISeparatorViewModel>
{
    public VerticalSeparatorView()
    {
        InitializeComponent();
        
        this.WhenActivated(d =>
        {
            var panelABounds = this.WhenAnyValue(view => view.ViewModel!.PaneA.ActualBounds);
            var panelBBounds = this.WhenAnyValue(view => view.ViewModel!.PaneB.ActualBounds);

            panelBBounds.Select(view => view.Height - Height / 2)
                .OnUI()
                .Subscribe(val => Canvas.SetTop(this, val))
                .DisposeWith(d);

            panelABounds.CombineLatest(panelBBounds)
                .Select(panels =>
                {
                    var (panelA, panelB) = panels;

                    var biggestLeft = Math.Max(panelA.X, panelB.X);
                    var smallestRight = Math.Min(panelA.Right, panelB.Right);

                    var top = biggestLeft + ((smallestRight - biggestLeft) / 2) - (Height / 2);
                    return top;
                })
                .OnUI()
                .Subscribe(val => Canvas.SetLeft(this, val))
                .DisposeWith(d);

        });
    }
}

