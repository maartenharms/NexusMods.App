using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class HorizontalSeparatorView : ReactiveUserControl<ISeparatorViewModel>
{
    private bool _isPressed;
    private Point _originalPosition;

    public HorizontalSeparatorView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            var panelABounds = this.WhenAnyValue(view => view.ViewModel!.PaneA.ActualBounds);
            var panelBBounds = this.WhenAnyValue(view => view.ViewModel!.PaneB.ActualBounds);

            panelBBounds.Select(view => view.Width - Width / 2)
                .OnUI()
                .Subscribe(val => Canvas.SetLeft(this, val))
                .DisposeWith(d);

            panelABounds.CombineLatest(panelBBounds)
                .Select(panels =>
                {
                    var (panelA, panelB) = panels;

                    var biggestTop = Math.Max(panelA.Y, panelB.Y);
                    var smallestBottom = Math.Min(panelA.Bottom, panelB.Bottom);

                    var top = biggestTop + ((smallestBottom - biggestTop) / 2) - (Height / 2);
                    return top;
                })
                .OnUI()
                .Subscribe(val => Canvas.SetTop(this, val))
                .DisposeWith(d);

        });
    }

    private void MoveIcon_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Console.WriteLine("Mouse Down");
        _isPressed = true;
        //FlyoutBase.ShowAttachedFlyout(this);
        _originalPosition = e.GetPosition(Parent! as Control);
        
        base.OnPointerPressed(e);
    }

    private void MoveIcon_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        
        if (_isPressed)
        {
            var pos = e.GetPosition(Parent! as Control);
            var delta = _originalPosition - pos;
            ViewModel!.Move(delta.X);
            _originalPosition = pos;
            Console.WriteLine($"Moved {delta}, pos {pos}");
        }
        base.OnPointerMoved(e);
    }

    private void MoveIcon_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPressed = false;
        
        Console.WriteLine($"Released {_originalPosition}");
        base.OnPointerReleased(e);
    }
}

