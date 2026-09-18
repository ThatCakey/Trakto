using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Visive;

namespace Trakto.ViewModels;

public partial class ProjectSession : ObservableObject
{
    public static ProjectSession Current { get; } = new();

    [ObservableProperty]
    private VideoObject? _mainTimeline;

    [ObservableProperty]
    private float _currentTime;

    public ObservableCollection<VideoObject> MediaBin { get; } = new();

    [ObservableProperty]
    private object? _selectedItem;

    private ProjectSession()
    {
        // Initialize an empty timeline (1080p, 30fps)
        _mainTimeline = new VideoObject("Timeline", new System.Numerics.Vector2(1920, 1080), 30f);
    }
}
