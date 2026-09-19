using System;
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

    private WriteableBitmap? _pingBitmap;
    private WriteableBitmap? _pongBitmap;
    private bool _usePing = true;

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

    private System.Threading.CancellationTokenSource? _debounceCts;

    private void UpdatePreview()
    {
        if (_session.MainTimeline == null) return;
        
        bool isPlaying = _session.IsPlaying;
        
        // If we are NOT playing (scrubbing/seeking), use keyframeOnly = true for speed.
        // If we are playing, use keyframeOnly = false for smooth playback, and quality = 1.0f to avoid slow CPU resizing.
        // We pass allowSync = !isPlaying so that during playback, cache misses return null immediately instead of freezing the UI.
        try
        {
            using (var frameObj = _session.MainTimeline.GetPreviewFrame(_session.CurrentTime, isPlaying ? 1.0f : 0.5f, !isPlaying, !isPlaying))
            {
                if (frameObj != null && frameObj.loaded)
                {
                    var bitmap = _usePing ? _pingBitmap : _pongBitmap;
                    VisiveBridge.UpdateBitmap(ref bitmap, frameObj);
                    if (_usePing) _pingBitmap = bitmap;
                    else _pongBitmap = bitmap;
                    _usePing = !_usePing;

                    PreviewImage = bitmap;
                    OnPropertyChanged(nameof(PreviewImage));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdatePreview failed: {ex.Message}");
        }

        // Debounce high-quality fetch (and prefetch trigger) when paused/scrubbing
        _debounceCts?.Cancel();
        if (!isPlaying)
        {
            _debounceCts = new System.Threading.CancellationTokenSource();
            var token = _debounceCts.Token;
            float targetTime = _session.CurrentTime;
            
            System.Threading.Tasks.Task.Delay(300, token).ContinueWith(t => 
            {
                if (t.IsCanceled || _session.CurrentTime != targetTime) return;
                
                // Requesting a high-quality frame with keyframeOnly = false triggers 
                // the background chunk extraction in Visive for this exact spot!
                // We pass allowSync = true because the user is paused and we absolutely want this high-quality frame.
                using (var hqFrame = _session.MainTimeline.GetPreviewFrame(targetTime, 1.0f, false, true))
                {
                    if (hqFrame != null && hqFrame.loaded)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => 
                        {
                            if (!token.IsCancellationRequested && _session.CurrentTime == targetTime)
                            {
                                var bitmap = _usePing ? _pingBitmap : _pongBitmap;
                                VisiveBridge.UpdateBitmap(ref bitmap, hqFrame);
                                if (_usePing) _pingBitmap = bitmap;
                                else _pongBitmap = bitmap;
                                _usePing = !_usePing;

                                PreviewImage = bitmap;
                            }
                        }).Wait(); // Wait so we don't dispose the frame before it's converted
                    }
                }
            });
        }
    }
}
