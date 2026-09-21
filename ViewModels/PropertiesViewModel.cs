using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Trakto.ViewModels.Timeline;

namespace Trakto.ViewModels;

public partial class PropertiesViewModel : ObservableObject
{
    private readonly ProjectSession _session;

    [ObservableProperty]
    private object? _selectedItem;

    public TimelineClipViewModel? SelectedClip => SelectedItem as TimelineClipViewModel;

    public bool IsClipSelected => SelectedItem is TimelineClipViewModel;

    public PropertiesViewModel()
    {
        _session = ProjectSession.Current;
        SelectedItem = _session.SelectedItem;

        _session.PropertyChanged += OnSessionPropertyChanged;
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectSession.SelectedItem))
        {
            SelectedItem = _session.SelectedItem;
            OnPropertyChanged(nameof(IsClipSelected));
            OnPropertyChanged(nameof(SelectedClip));
        }
    }
}
