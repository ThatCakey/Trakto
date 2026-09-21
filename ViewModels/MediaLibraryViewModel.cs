using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Visive;

namespace Trakto.ViewModels;

public partial class MediaLibraryViewModel : ObservableObject
{
    private readonly ProjectSession _session;

    public ObservableCollection<VideoObject> MediaItems => _session.MediaBin;

    public MediaLibraryViewModel()
    {
        _session = ProjectSession.Current;
    }
}
