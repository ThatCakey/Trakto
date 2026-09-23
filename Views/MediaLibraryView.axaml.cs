using Avalonia.Controls;

namespace Trakto.Views;

public partial class MediaLibraryView : UserControl
{
    private Avalonia.Point _dragStartPoint;
    private bool _isDragging;

    public MediaLibraryView()
    {
        InitializeComponent();
    }

    private Avalonia.Input.PointerPressedEventArgs? _dragStartEventArgs;

    private void OnItemPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _dragStartPoint = e.GetPosition(this);
            _dragStartEventArgs = e;
            _isDragging = false;
        }
    }

    public static readonly Avalonia.Input.DataFormat<Visive.VideoObject> VideoObjectFormat = Avalonia.Input.DataFormat.CreateInProcessFormat<Visive.VideoObject>("VideoObject");

    private async void OnItemPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !_isDragging)
        {
            var point = e.GetPosition(this);
            var delta = point - _dragStartPoint;
            
            // Start drag if moved past threshold
            if (System.Math.Abs(delta.X) > 3 || System.Math.Abs(delta.Y) > 3)
            {
                _isDragging = true;
                
                if (sender is Control control && control.DataContext is Visive.VideoObject video && _dragStartEventArgs != null)
                {
                    var data = new Avalonia.Input.DataTransfer();
                    var item = new Avalonia.Input.DataTransferItem();
                    item.Set(VideoObjectFormat, video);
                    data.Add(item);
                    
                    var result = await Avalonia.Input.DragDrop.DoDragDropAsync(_dragStartEventArgs, data, Avalonia.Input.DragDropEffects.Copy);
                    _isDragging = false; // Reset after drag finishes
                }
            }
        }
    }
}
