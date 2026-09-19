using CommunityToolkit.Mvvm.ComponentModel;
using Visive;

namespace Trakto.ViewModels.Timeline;

public partial class TimelineClipViewModel : ObservableObject
{
    private readonly VideoClip _clip;
    private readonly float _fps;

    [ObservableProperty]
    private string _name;

    // Track the timeline's zoom level to update X and Width
    private double _pixelsPerSecond = 100.0;
    
    public double PixelsPerSecond
    {
        get => _pixelsPerSecond;
        set
        {
            if (SetProperty(ref _pixelsPerSecond, value))
            {
                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    public double X => (_clip.TimelineStartFrame / _fps) * PixelsPerSecond;
    public double Width => (_clip.Length / _fps) * PixelsPerSecond;
    
    public VideoClip BackendClip => _clip;

    public TimelineClipViewModel(VideoClip clip, string name, float fps)
    {
        _clip = clip;
        _name = name;
        _fps = fps;
    }
}
