using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aural;
using Visive;

namespace Trakto.ViewModels;

public class AudioEngine : IDisposable
{
    private class ActiveStream : IDisposable
    {
        public PlaybackStream Playback { get; }
        public FileStream File { get; }
        public AudioClip Clip { get; }

        public ActiveStream(AudioClip clip)
        {
            Clip = clip;
            Playback = Player.CreateStream(48000, 2);
            File = new FileStream(clip.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void SeekTo(float timeInSeconds)
        {
            float localTime = timeInSeconds - Clip.TimelineStartTime + Clip.SourceStartTime;
            if (localTime < 0) localTime = 0;
            if (localTime > Clip.Length) localTime = Clip.Length;

            // 48000 Hz * 2 channels * 2 bytes per sample = 192000 bytes per second
            long byteOffset = (long)(localTime * 48000 * 2 * 2);
            
            // Align to 4-byte boundary (1 stereo sample)
            byteOffset -= byteOffset % 4;

            File.Position = byteOffset;
            Playback.ClearQueue();
        }

        public void Dispose()
        {
            Playback.Dispose();
            File.Dispose();
        }
    }

    private readonly Dictionary<AudioClip, ActiveStream> _activeStreams = new();
    private bool _isDisposed;
    private readonly Thread _workerThread;
    private float _lastSeenTime;
    
    private const int ChunkSize = 2048; // 2KB chunks for lower latency

    public AudioEngine()
    {
        // Listen to project session
        ProjectSession.Current.PropertyChanged += OnSessionPropertyChanged;
        ProjectSession.Current.Scrubbed += OnSessionScrubbed;

        _workerThread = new Thread(WorkerLoop)
        {
            IsBackground = true,
            Name = "AudioEngine_Worker"
        };
        _workerThread.Start();
    }

    private void OnSessionScrubbed()
    {
        // If we are playing, the scrubber jumped, so we must seek streams
        var session = ProjectSession.Current;
        if (session.IsPlaying)
        {
            SeekAll(session.CurrentTime);
        }
    }

    private void OnSessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectSession.IsPlaying))
        {
            var session = ProjectSession.Current;
            if (session.IsPlaying)
            {
                StartPlayback(session.CurrentTime);
            }
            else
            {
                PausePlayback();
            }
        }
        else if (e.PropertyName == nameof(ProjectSession.CurrentTime))
        {
            var session = ProjectSession.Current;
            if (!session.IsPlaying)
            {
                SeekAll(session.CurrentTime);
            }
            _lastSeenTime = session.CurrentTime;
        }
        else if (e.PropertyName == nameof(ProjectSession.IsBuffering))
        {
            var session = ProjectSession.Current;
            if (session.IsPlaying)
            {
                if (session.IsBuffering)
                {
                    PausePlayback();
                }
                else
                {
                    StartPlayback(session.CurrentTime);
                }
            }
        }
    }

    private void StartPlayback(float time)
    {
        var session = ProjectSession.Current;
        if (session.MainTimeline == null) return;

        lock (_activeStreams)
        {
            // For now, we just sync with the MainTimeline's audio clips
            foreach (var clip in session.MainTimeline.audioClips)
            {
                if (time >= clip.TimelineStartTime && time < clip.TimelineStartTime + clip.Length)
                {
                    if (!_activeStreams.ContainsKey(clip))
                    {
                        var stream = new ActiveStream(clip);
                        _activeStreams[clip] = stream;
                    }
                    _activeStreams[clip].SeekTo(time);
                    _activeStreams[clip].Playback.Play();
                }
            }
        }
    }

    private void PausePlayback()
    {
        lock (_activeStreams)
        {
            foreach (var stream in _activeStreams.Values)
            {
                stream.Playback.Pause();
            }
        }
    }

    private void SeekAll(float time)
    {
        lock (_activeStreams)
        {
            // In a real editor, you'd prune streams that are out of bounds and create ones in bounds.
            // For simplicity, we just stop everything out of bounds, and seek anything active.
            var toRemove = new List<AudioClip>();

            foreach (var kvp in _activeStreams)
            {
                var clip = kvp.Key;
                var stream = kvp.Value;

                if (time < clip.TimelineStartTime || time >= clip.TimelineStartTime + clip.Length)
                {
                    toRemove.Add(clip);
                }
                else
                {
                    stream.SeekTo(time);
                }
            }

            foreach (var clip in toRemove)
            {
                _activeStreams[clip].Dispose();
                _activeStreams.Remove(clip);
            }
        }
    }

    private void WorkerLoop()
    {
        byte[] buffer = new byte[ChunkSize];

        while (!_isDisposed)
        {
            var session = ProjectSession.Current;
            if (!session.IsPlaying)
            {
                Thread.Sleep(10);
                continue;
            }

            lock (_activeStreams)
            {
                // Also check if we need to spawn new streams during playback
                if (session.MainTimeline != null)
                {
                    foreach (var clip in session.MainTimeline.audioClips)
                    {
                        float time = session.CurrentTime;
                        if (time >= clip.TimelineStartTime && time < clip.TimelineStartTime + clip.Length)
                        {
                            if (!_activeStreams.ContainsKey(clip))
                            {
                                var stream = new ActiveStream(clip);
                                stream.SeekTo(time);
                                stream.Playback.Play();
                                _activeStreams[clip] = stream;
                            }
                        }
                    }
                }

                // Feed active streams
                var toRemove = new List<AudioClip>();
                foreach (var kvp in _activeStreams)
                {
                    var clip = kvp.Key;
                    var stream = kvp.Value;

                    // If clip ended, mark for removal
                    if (session.CurrentTime >= clip.TimelineStartTime + clip.Length)
                    {
                        toRemove.Add(clip);
                        continue;
                    }

                    // Push a chunk if we can read one
                    int bytesRead = stream.File.Read(buffer, 0, ChunkSize);
                    if (bytesRead > 0)
                    {
                        byte[] data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        stream.Playback.EnqueueSamples(data);
                    }
                    else
                    {
                        // EOF reached
                        toRemove.Add(clip);
                    }
                }

                foreach (var clip in toRemove)
                {
                    _activeStreams[clip].Dispose();
                    _activeStreams.Remove(clip);
                }
            }

            Thread.Sleep(10); // Throttle
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
        ProjectSession.Current.PropertyChanged -= OnSessionPropertyChanged;
        ProjectSession.Current.Scrubbed -= OnSessionScrubbed;
        
        lock (_activeStreams)
        {
            foreach (var stream in _activeStreams.Values)
            {
                stream.Dispose();
            }
            _activeStreams.Clear();
        }
    }
}
