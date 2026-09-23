using Avalonia.Controls;

namespace Trakto.Views.Timeline;

public partial class TimelineClipView : UserControl
{
    public TimelineClipView()
    {
        InitializeComponent();
    }

    private Avalonia.Point _dragStartPoint;
    private float _initialStartTime;
    private bool _isDragging = false;

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DataContext is Trakto.ViewModels.Timeline.TimelineClipViewModel vm)
        {
            vm.SelectClip();
            
            var properties = e.GetCurrentPoint(this).Properties;
            if (properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(this.Parent as Avalonia.Visual);
                _initialStartTime = vm.StartTime;
                e.Pointer.Capture(sender as Avalonia.Input.IInputElement);
            }
        }
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (_isDragging && DataContext is Trakto.ViewModels.Timeline.TimelineClipViewModel vm)
        {
            var point = e.GetPosition(this.Parent as Avalonia.Visual);
            double deltaX = point.X - _dragStartPoint.X;
            
            float timeDelta = (float)(deltaX / vm.PixelsPerSecond);
            vm.StartTime = _initialStartTime + timeDelta;
            
            // Multitrack snapping check will go here when multiple tracks are fully supported
        }
    }

    private void OnPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
            
            if (DataContext is Trakto.ViewModels.Timeline.TimelineClipViewModel vm)
            {
                // Here we would finalize any multitrack transfer if the Y delta moved it to a new track
            }
        }
    }
}
