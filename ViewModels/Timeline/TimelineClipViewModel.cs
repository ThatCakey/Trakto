using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public float XPos
    {
        get => _clip.Position.X;
        set
        {
            if (_clip.Position.X != value)
            {
                _clip.Position.X = value;
                OnPropertyChanged();
                ProjectSession.Current.RequestPreviewRefresh();
            }
        }
    }

    public float YPos
    {
        get => _clip.Position.Y;
        set
        {
            if (_clip.Position.Y != value)
            {
                _clip.Position.Y = value;
                OnPropertyChanged();
                ProjectSession.Current.RequestPreviewRefresh();
            }
        }
    }

    public float ClipWidth
    {
        get => _clip.Resolution.X;
        set
        {
            if (_clip.Resolution.X != value)
            {
                _clip.Resolution.X = value;
                OnPropertyChanged();
                ProjectSession.Current.RequestPreviewRefresh();
            }
        }
    }

    public float ClipHeight
    {
        get => _clip.Resolution.Y;
        set
        {
            if (_clip.Resolution.Y != value)
            {
                _clip.Resolution.Y = value;
                OnPropertyChanged();
                ProjectSession.Current.RequestPreviewRefresh();
            }
        }
    }

    [RelayCommand]
    public void SelectClip()
    {
        ProjectSession.Current.SelectedItem = this;
    }

    [ObservableProperty]
    private bool _isSelected;

    public TimelineClipViewModel(VideoClip clip, string name, float fps)
    {
        _clip = clip;
        _name = name;
        _fps = fps;

        ProjectSession.Current.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ProjectSession.SelectedItem))
            {
                IsSelected = ProjectSession.Current.SelectedItem == this;
            }
        };
    }
}
