using Avalonia.Controls;
using Trakto.ViewModels;

namespace Trakto.Views;

public partial class PreviewView : UserControl
{
    public PreviewView()
    {
        InitializeComponent();
        DataContext = new PreviewViewModel();
    }
}
