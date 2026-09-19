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

    [ObservableProperty]
    private bool _isPlaying;

    public ObservableCollection<VideoObject> MediaBin { get; } = new();

    [ObservableProperty]
    private object? _selectedItem;

    private ProjectSession()
    {
        string assetsPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets");
        string testVideoPath = System.IO.Path.Combine(assetsPath, "test.mp4");

        if (System.IO.File.Exists(testVideoPath))
        {
            _mainTimeline = new VideoObject("test", testVideoPath);
            MediaBin.Add(_mainTimeline);
        }
        else
        {
            // Initialize an empty timeline (1080p, 30fps)
            _mainTimeline = new VideoObject("Timeline", new System.Numerics.Vector2(1920, 1080), 30f);
        }
    }
}
