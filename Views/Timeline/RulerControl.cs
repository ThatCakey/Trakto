using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Trakto.Views.Timeline;

public class RulerControl : Control
{
    public static readonly StyledProperty<double> PixelsPerSecondProperty =
        AvaloniaProperty.Register<RulerControl, double>(nameof(PixelsPerSecond), 100.0);

    public double PixelsPerSecond
    {
        get => GetValue(PixelsPerSecondProperty);
        set => SetValue(PixelsPerSecondProperty, value);
    }

    public static readonly StyledProperty<double> MaxTimeProperty =
        AvaloniaProperty.Register<RulerControl, double>(nameof(MaxTime), 300.0);

    public double MaxTime
    {
        get => GetValue(MaxTimeProperty);
        set => SetValue(MaxTimeProperty, value);
    }

    private readonly IPen _majorTickPen;
    private readonly IPen _minorTickPen;

    public RulerControl()
    {
        _majorTickPen = new Pen(new SolidColorBrush(Color.Parse("#d3d3d3")), 1); // LightGray
        _minorTickPen = new Pen(new SolidColorBrush(Color.Parse("#808080")), 1); // Gray
        
        ClipToBounds = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == PixelsPerSecondProperty || change.Property == MaxTimeProperty)
        {
            InvalidateVisual(); // Request a redraw when zoom or time changes
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double pps = PixelsPerSecond;
        if (pps <= 0) return;

        // Determine spacing: we want a major tick roughly every 100 pixels.
        // Use integer math to guarantee precise divisions.
        double targetSeconds = 100.0 / pps;
        
        int secondsPerMajor = (int)Math.Max(1, Math.Round(targetSeconds));
        // Common intervals: 1s, 2s, 5s, 10s, 30s, 60s
        if (secondsPerMajor > 1 && secondsPerMajor < 5) secondsPerMajor = 2;
        else if (secondsPerMajor > 5 && secondsPerMajor < 10) secondsPerMajor = 5;
        else if (secondsPerMajor > 10 && secondsPerMajor < 30) secondsPerMajor = 10;
        else if (secondsPerMajor > 30 && secondsPerMajor < 60) secondsPerMajor = 30;

        int minorTicksPerMajor = 5;
        double secondsPerMinor = (double)secondsPerMajor / minorTicksPerMajor;

        // Since the ruler is inside a ScrollViewer, it is actually rendered at its full width.
        // For absolute performance with massive timelines, we could use Bounds and ScrollOffset 
        // to only draw visible ticks, but DrawingContext clipping is fast enough for now.
        
        double maxSeconds = MaxTime + (secondsPerMajor * 2); 
        double controlHeight = Bounds.Height;

        int totalMinorTicks = (int)(maxSeconds / secondsPerMinor);

        for (int i = 0; i <= totalMinorTicks; i++)
        {
            double t = i * secondsPerMinor;
            double x = t * pps;

            bool isMajor = (i % minorTicksPerMajor == 0);

            if (isMajor)
            {
                // Draw major line
                context.DrawLine(_majorTickPen, new Point(x, controlHeight - 15), new Point(x, controlHeight));
                
                // Draw text
                var formattedText = new FormattedText(
                    TimeSpan.FromSeconds(Math.Round(t)).ToString(@"m\:ss"),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    10,
                    _majorTickPen.Brush);
                    
                context.DrawText(formattedText, new Point(x + 3, controlHeight - 28));
            }
            else
            {
                // Draw minor line
                context.DrawLine(_minorTickPen, new Point(x, controlHeight - 10), new Point(x, controlHeight));
            }
        }
    }
}
