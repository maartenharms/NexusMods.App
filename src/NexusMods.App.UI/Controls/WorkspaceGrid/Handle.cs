using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Logging;

namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class Handle : Border
{
    private bool _isPressed;
    private readonly WorkspaceGrid _parent;
    private Point _originalPosition;
    private readonly ParametrizedLogger? _logger;
    private readonly SeparatorHandle _separatorHandle;

    public Handle(WorkspaceGrid parent, SeparatorHandle separatorHandle)
    {
        _parent = parent;
        _separatorHandle = separatorHandle;
        Console.WriteLine("bleh");
   
    }
    
    public Direction Direction { get; init; }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _isPressed = true;
        _originalPosition = e.GetPosition(_parent);
        
        Console.WriteLine($"Pressed {_originalPosition}");
        base.OnPointerPressed(e);
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isPressed = false;
        
        Console.WriteLine($"Released {_originalPosition}");
        base.OnPointerReleased(e);
    }
    
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        
        if (_isPressed)
        {
            var pos = e.GetPosition(_parent);
            var delta = _originalPosition - pos;
            _parent.MoveHandle(_separatorHandle, delta);
            _originalPosition = pos;
            Console.WriteLine($"Moved {delta}, pos {pos}");
        }
        base.OnPointerMoved(e);
    }
}
