using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Trakto.ViewModels.Timeline;

public partial class TimelineTrackViewModel : ObservableObject
{
    [ObservableProperty]
    private string _trackName;

    public ObservableCollection<TimelineClipViewModel> Clips { get; } = new();

    public TimelineTrackViewModel(string trackName)
    {
        _trackName = trackName;
    }

    public void UpdateZoomLevel(double pixelsPerSecond)
    {
        foreach (var clip in Clips)
        {
            clip.PixelsPerSecond = pixelsPerSecond;
        }
    }
}
