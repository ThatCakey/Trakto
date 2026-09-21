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

    [ObservableProperty]
    private bool _isBuffering;

    public ObservableCollection<VideoObject> MediaBin { get; } = new();

    [ObservableProperty]
    private object? _selectedItem;

    public event System.Action? RefreshPreviewRequested;
    public event System.Action? Scrubbed;

    public void RequestPreviewRefresh()
    {
        RefreshPreviewRequested?.Invoke();
    }

    public void NotifyScrubbed()
    {
        Scrubbed?.Invoke();
    }

    private ProjectSession()
    {
        string assetsPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets");
        string testVideoPath = System.IO.Path.Combine(assetsPath, "test.mp4");

        if (System.IO.File.Exists(testVideoPath))
        {
            _mainTimeline = new VideoObject("test", testVideoPath);
            _mainTimeline.ChunkLoaded += () => Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => RequestPreviewRefresh());
            MediaBin.Add(_mainTimeline);
        }
        else
        {
            // Initialize an empty timeline (1080p, 30fps)
            _mainTimeline = new VideoObject("Timeline", new System.Numerics.Vector2(1920, 1080), 30f);
            _mainTimeline.ChunkLoaded += () => Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => RequestPreviewRefresh());
        }
    }
}
