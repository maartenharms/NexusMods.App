using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public partial class VerticalSeparatorView : ReactiveUserControl<ISeparatorViewModel>
{
    private bool _isPressed;
    private Point _originalPosition;

    public VerticalSeparatorView()
    {
        InitializeComponent();
        
        this.WhenActivated(d =>
        {
            var panelABounds = this.WhenAnyValue(view => view.ViewModel!.PaneA.ActualBounds);
            var panelBBounds = this.WhenAnyValue(view => view.ViewModel!.PaneB.ActualBounds);

            panelBBounds
                .OnUI()
                .Select(view => view.Top - Height / 2)
                .Subscribe(val => Canvas.SetTop(this, val))
                .DisposeWith(d);

            panelABounds
                .CombineLatest(panelBBounds)
                .OnUI()
                .Select(panels =>
                {
                    var (panelA, panelB) = panels;

                    var biggestLeft = Math.Max(panelA.X, panelB.X);
                    var smallestRight = Math.Min(panelA.Right, panelB.Right);

                    var top = biggestLeft + ((smallestRight - biggestLeft) / 2) - (Width / 2);
                    return top;
                })
                .Subscribe(val => Canvas.SetLeft(this, val))
                .DisposeWith(d);
            
            SplitTopButton.Command = ReactiveCommand.Create(() => ViewModel!.SplitA());
            SplitBottomButton.Command = ReactiveCommand.Create(() => ViewModel!.SplitB());
            SwapPanelsButton.Command = ReactiveCommand.Create(() => ViewModel!.Swap());
            
            JoinTopButton.Command = ReactiveCommand.Create(() => ViewModel!.JoinAToB(),
                this.WhenAnyValue(view => view.ViewModel!.CanJoin));
            
            JoinBottomButton.Command = ReactiveCommand.Create(() => ViewModel!.JoinBToA(),
                this.WhenAnyValue(view => view.ViewModel!.CanJoin));

            this.WhenAnyValue(view => view.ViewModel!.Workspace.EditMode)
                .BindToUi(this, view => view.IsVisible)
                .DisposeWith(d);

        });

    }
    
            
        
    private void MoveIcon_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
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
            ViewModel!.Move(delta.Y);
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

