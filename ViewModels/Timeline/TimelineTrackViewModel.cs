using System.Collections.ObjectModel;
using System.Linq;
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

    public void AddClip(Visive.VideoObject video, float startTime)
    {
        var session = ProjectSession.Current;
        if (session.MainTimeline == null || video.clips.Count == 0) return;

        var sourceClip = video.clips[0];

        // In a true multitrack system, each track would represent a layer. 
        // For now, Trakto has a flat list of clips in MainTimeline.
        var newClip = new Visive.VideoClip
        {
            SourcePath = sourceClip.SourcePath,
            SourceStartFrame = 0,
            SourceEndFrame = (uint)(video.length * session.MainTimeline.fps),
            TimelineStartFrame = (uint)(startTime * session.MainTimeline.fps),
            Resolution = video.resolution
        };
        session.MainTimeline.clips.Add(newClip);
        // Also add audio clip if it exists
        session.MainTimeline.audioClips.Add(new Visive.AudioClip
        {
            SourcePath = sourceClip.SourcePath,
            SourceStartTime = 0f,
            SourceEndTime = video.length,
            TimelineStartTime = startTime
        });

        // Create UI ViewModel
        double zoom = 100.0;
        if (Clips.Count > 0) zoom = Clips[0].PixelsPerSecond;
        else
        {
            // Fallback: we could inject the TimelineViewModel but for now we can just read it if needed.
            // Or use a default of 100.0 until it gets updated.
        }
        
        var clipVm = new TimelineClipViewModel(newClip, video.Name, session.MainTimeline.fps)
        {
            PixelsPerSecond = zoom,
            ParentTrack = this
        };
        
        Clips.Add(clipVm);
        session.NotifyScrubbed(); // Force preview update
    }

    public void RemoveClip(TimelineClipViewModel clipVm)
    {
        var session = ProjectSession.Current;
        if (session.MainTimeline != null)
        {
            session.MainTimeline.clips.Remove(clipVm.Clip);
            
            // Remove corresponding audio clip
            var audioClip = session.MainTimeline.audioClips.FirstOrDefault(a => a.SourcePath == clipVm.Clip.SourcePath && a.TimelineStartTime == clipVm.Clip.TimelineStartFrame / session.MainTimeline.fps);
            if (audioClip != null)
            {
                session.MainTimeline.audioClips.Remove(audioClip);
            }
        }
        Clips.Remove(clipVm);
        session.NotifyScrubbed();
    }

    public void TransferClipTo(TimelineClipViewModel clipVm, TimelineTrackViewModel newTrack)
    {
        if (this == newTrack) return;
        
        Clips.Remove(clipVm);
        clipVm.ParentTrack = newTrack;
        newTrack.Clips.Add(clipVm);
        
        ProjectSession.Current.NotifyScrubbed();
    }
}
