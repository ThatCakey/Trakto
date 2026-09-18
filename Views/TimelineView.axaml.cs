using Avalonia.Controls;
using Trakto.ViewModels;

namespace Trakto.Views;

public partial class TimelineView : UserControl
{
    public TimelineView()
    {
        InitializeComponent();
        DataContext = new TimelineViewModel();
    }
}
