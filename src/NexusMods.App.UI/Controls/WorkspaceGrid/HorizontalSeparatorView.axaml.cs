using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

            panelBBounds.Select(view => view.Left - Width / 2)
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

            SplitLeftButton.Command = ReactiveCommand.Create(() => ViewModel!.SplitA());
            SplitRightButton.Command = ReactiveCommand.Create(() => ViewModel!.SplitB());
            SwapPanelsButton.Command = ReactiveCommand.Create(() => ViewModel!.Swap());

        });
    }

    private void MoveIcon_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Console.WriteLine("Mouse Down");
        _isPressed = true;
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
        }
        base.OnPointerMoved(e);
    }

    private void MoveIcon_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPressed = false;
        
        base.OnPointerReleased(e);
    }
}

