using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Trakto.ViewModels;

namespace Trakto.Views;

public partial class TimelineView : UserControl
{
    private bool _isScrubbing = false;
    private bool _wasPlayingBeforeScrub = false;

    public TimelineView()
    {
        InitializeComponent();
        DataContext = new TimelineViewModel();
    }

    private void OnRulerPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is TimelineViewModel vm)
        {
            _isScrubbing = true;
            _wasPlayingBeforeScrub = vm.IsPlaying;
            if (_wasPlayingBeforeScrub)
            {
                vm.Pause();
            }

            e.Pointer.Capture(sender as IInputElement);
            
            var point = e.GetPosition(sender as Visual);
            UpdatePlayhead(vm, point.X);
        }
    }

    private void OnRulerPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isScrubbing && DataContext is TimelineViewModel vm)
        {
            var point = e.GetPosition(sender as Visual);
            UpdatePlayhead(vm, point.X);
        }
    }

    private void OnRulerPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isScrubbing)
        {
            _isScrubbing = false;
            e.Pointer.Capture(null);

            if (_wasPlayingBeforeScrub && DataContext is TimelineViewModel vm)
            {
                vm.Play();
            }
        }
    }

    private void UpdatePlayhead(TimelineViewModel vm, double mouseX)
    {
        // mouseX is relative to the ruler canvas, which means 0 is timeline start (150px offset handled by layout)
        double time = mouseX / vm.PixelsPerSecond;
        if (time < 0) time = 0;
        if (time > vm.MaxTime) time = vm.MaxTime;
        
        vm.ScrubTo((float)time);
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not TimelineViewModel vm) return;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            // Zoom in/out based on wheel delta Y
            double zoomDelta = e.Delta.Y > 0 ? 10 : -10;
            double newZoom = vm.PixelsPerSecond + zoomDelta;
            
            if (newZoom < 10) newZoom = 10;
            if (newZoom > 500) newZoom = 500;
            
            vm.PixelsPerSecond = newZoom;
            e.Handled = true;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            // Pan horizontally
            if (MainScroll != null)
            {
                double panDelta = e.Delta.Y > 0 ? -50 : 50;
                MainScroll.Offset = MainScroll.Offset.WithX(System.Math.Max(0, MainScroll.Offset.X + panDelta));
                e.Handled = true;
            }
        }
    }
}
