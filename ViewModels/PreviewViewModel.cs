using System.ComponentModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Visive;

namespace Trakto.ViewModels;

public partial class PreviewViewModel : ObservableObject
{
    private readonly ProjectSession _session;

    [ObservableProperty]
    private WriteableBitmap? _previewImage;

    public PreviewViewModel()
    {
        _session = ProjectSession.Current;
        _session.PropertyChanged += OnSessionPropertyChanged;
        
        // Render initial frame
        UpdatePreview();
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectSession.CurrentTime) || 
            e.PropertyName == nameof(ProjectSession.MainTimeline))
        {
            UpdatePreview();
        }
    }

    private void UpdatePreview()
    {
        if (_session.MainTimeline == null) return;
        
        // Use Visive to fetch the preview frame (at 50% quality for performance during scrubber)
        var frameObj = _session.MainTimeline.GetPreviewFrame(_session.CurrentTime, 0.5f);
        if (frameObj != null && frameObj.loaded)
        {
            PreviewImage = VisiveBridge.FrameToBitmap(frameObj);
        }
    }
}
