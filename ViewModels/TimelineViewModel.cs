using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Trakto.ViewModels;

public partial class TimelineViewModel : ObservableObject
{
    private readonly ProjectSession _session;

    public float MaxTime => _session.MainTimeline?.length ?? 100f;

    public float CurrentTime
    {
        get => _session.CurrentTime;
        set
        {
            if (_session.CurrentTime != value)
            {
                _session.CurrentTime = value;
                OnPropertyChanged();
            }
        }
    }

    public TimelineViewModel()
    {
        _session = ProjectSession.Current;
        _session.PropertyChanged += OnSessionPropertyChanged;
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectSession.CurrentTime))
        {
            OnPropertyChanged(nameof(CurrentTime));
        }
        else if (e.PropertyName == nameof(ProjectSession.MainTimeline))
        {
            OnPropertyChanged(nameof(MaxTime));
        }
    }
}
