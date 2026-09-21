using Avalonia.Controls;

namespace Trakto.Views.Timeline;

public partial class TimelineClipView : UserControl
{
    public TimelineClipView()
    {
        InitializeComponent();
    }

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DataContext is Trakto.ViewModels.Timeline.TimelineClipViewModel vm)
        {
            vm.SelectClip();
        }
        e.Handled = true;
    }
}
