using System;
using System.ComponentModel;
using Avalonia.Threading;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Trakto.ViewModels;

public partial class TimelineViewModel : ObservableObject
{
    private readonly ProjectSession _session;

    public float MaxTime => Math.Max(_session.MainTimeline?.length ?? 0f, 300f);

    [ObservableProperty]
    private double _pixelsPerSecond = 100.0;

    public double PlayheadX => (CurrentTime * PixelsPerSecond) + 150;
    
    public double TimelineWidth => (MaxTime * PixelsPerSecond) + 150;

    public System.Collections.ObjectModel.ObservableCollection<Timeline.TimelineTrackViewModel> Tracks { get; } = new();

    public float CurrentTime
    {
        get => _session.CurrentTime;
        set
        {
            if (_session.CurrentTime != value)
            {
                _session.CurrentTime = value;
                _lastTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayheadX));
            }
        }
    }

    public void ScrubTo(float time)
    {
        CurrentTime = time;
        _session.NotifyScrubbed();
    }

    public bool IsPlaying
    {
        get => _session.IsPlaying;
        set
        {
            if (_session.IsPlaying != value)
            {
                _session.IsPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotPlaying));
            }
        }
    }

    public bool IsNotPlaying => !IsPlaying;

    private DispatcherTimer? _playbackTimer;
    private Stopwatch _stopwatch = new();
    private float _lastTime;

    public TimelineViewModel()
    {
        _session = ProjectSession.Current;
        _session.PropertyChanged += OnSessionPropertyChanged;
        
        // Setup playback timer (approx 60fps)
        _playbackTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _playbackTimer.Tick += OnPlaybackTick;
        
        // Initialize tracks
        var mainTrack = new Timeline.TimelineTrackViewModel("V1 (Main)");
        Tracks.Add(mainTrack);
        Tracks.Add(new Timeline.TimelineTrackViewModel("V2"));
        Tracks.Add(new Timeline.TimelineTrackViewModel("V3"));
        
        // Populate dummy clip if a timeline exists and has clips
        if (_session.MainTimeline != null && _session.MainTimeline.clips.Count > 0)
        {
            foreach (var clip in _session.MainTimeline.clips)
            {
                mainTrack.Clips.Add(new Timeline.TimelineClipViewModel(clip, "Video Clip", _session.MainTimeline.fps) 
                { 
                    PixelsPerSecond = this.PixelsPerSecond,
                    ParentTrack = mainTrack
                });
            }
        }
    }

    partial void OnPixelsPerSecondChanged(double value)
    {
        OnPropertyChanged(nameof(PlayheadX));
        OnPropertyChanged(nameof(TimelineWidth));
        foreach (var track in Tracks)
        {
            track.UpdateZoomLevel(value);
        }
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectSession.CurrentTime))
        {
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(PlayheadX));
        }
        else if (e.PropertyName == nameof(ProjectSession.MainTimeline))
        {
            OnPropertyChanged(nameof(MaxTime));
            OnPropertyChanged(nameof(TimelineWidth));
        }
        else if (e.PropertyName == nameof(ProjectSession.IsPlaying))
        {
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsNotPlaying));
        }
    }

    [RelayCommand]
    public void Play()
    {
        if (IsPlaying) return;
        
        if (CurrentTime >= MaxTime)
        {
            CurrentTime = 0; // Restart if at end
        }
        
        IsPlaying = true;
        _lastTime = CurrentTime;
        _stopwatch.Restart();
        _playbackTimer?.Start();
    }

    [RelayCommand]
    public void Pause()
    {
        if (!IsPlaying) return;
        
        IsPlaying = false;
        _stopwatch.Stop();
        _playbackTimer?.Stop();
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        if (!IsPlaying) return;
        
        if (_session.IsBuffering)
        {
            _stopwatch.Restart(); // Prevent time accumulation during buffer stall
            return;
        }

        float delta = (float)_stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();
        
        float newTime = _lastTime + delta;
        if (newTime >= MaxTime)
        {
            newTime = MaxTime;
            Pause();
        }
        
        CurrentTime = newTime;
        _lastTime = newTime;
    }
}
