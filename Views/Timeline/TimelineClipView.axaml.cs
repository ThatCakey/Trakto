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
                // Multitrack transfer
                // Find TracksControl by walking up the visual tree
                var control = this.Parent as Avalonia.Controls.Control;
                Avalonia.Controls.ItemsControl? tracksControl = null;
                while (control != null)
                {
                    if (control is Avalonia.Controls.ItemsControl ic && ic.Name == "TracksControl")
                    {
                        tracksControl = ic;
                        break;
                    }
                    control = control.Parent as Avalonia.Controls.Control;
                }

                if (tracksControl != null && tracksControl.DataContext is Trakto.ViewModels.TimelineViewModel timelineVm)
                {
                    var point = e.GetPosition(tracksControl);
                    int trackIndex = (int)(point.Y / 60); // 60 is the track height
                    
                    if (trackIndex >= 0 && trackIndex < timelineVm.Tracks.Count)
                    {
                        var newTrack = timelineVm.Tracks[trackIndex];
                        if (vm.ParentTrack != null && newTrack != vm.ParentTrack)
                        {
                            vm.ParentTrack.TransferClipTo(vm, newTrack);
                        }
                    }
                }
            }
        }
    }
}
