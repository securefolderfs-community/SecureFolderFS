using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    /// <summary>
    /// The dashboard feature slide, showing a live mock of the vault dashboard:
    /// a health status card and a continuously flowing read-speed graph.
    /// </summary>
    public sealed partial class DashboardSlide : UserControl
    {
        private const int SAMPLE_COUNT = 48;
        private const int FRAME_INTERVAL_MS = 100;
        private const double MIN_SPEED = 40d;
        private const double MAX_SPEED = 280d;

        private readonly DispatcherTimer _frameTimer;
        private double _time;
        private int _ticksSinceLabelUpdate;

        public DashboardSlide()
        {
            InitializeComponent();

            _frameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FRAME_INTERVAL_MS) };
            _frameTimer.Tick += FrameTimer_Tick;

            Loaded += DashboardSlide_Loaded;
            Unloaded += DashboardSlide_Unloaded;
        }

        /// <summary>
        /// Produces a smooth pseudo-random sine wave.
        /// </summary>
        private static double SpeedAt(double time)
        {
            var wave =
                Math.Sin(time * 0.9) * 0.45 +
                Math.Sin(time * 2.3 + 1.3) * 0.3 +
                Math.Sin(time * 4.1 + 0.7) * 0.15;

            var normalized = (wave + 0.9) / 1.8; // roughly 0..1
            return MIN_SPEED + normalized * (MAX_SPEED - MIN_SPEED);
        }

        private void FrameTimer_Tick(object? sender, object e)
        {
            _time += FRAME_INTERVAL_MS / 1000d;
            RebuildChart();

            // Refresh the label less often so the number stays readable
            if (++_ticksSinceLabelUpdate >= 5)
            {
                _ticksSinceLabelUpdate = 0;
                SpeedText.Text = $"{(int)SpeedAt(_time)}mb/s";
            }
        }

        private void RebuildChart()
        {
            var width = ChartHost.ActualWidth;
            var height = ChartHost.ActualHeight;
            if (width <= 0d || height <= 0d)
                return;

            var linePoints = new PointCollection();
            var areaPoints = new PointCollection();

            for (var i = 0; i < SAMPLE_COUNT; i++)
            {
                // The window ends at the current time, so the curve scrolls right to left
                var sampleTime = _time - (SAMPLE_COUNT - 1 - i) * 0.18;
                var normalized = (SpeedAt(sampleTime) - MIN_SPEED) / (MAX_SPEED - MIN_SPEED);

                var point = new Point(
                    i / (double)(SAMPLE_COUNT - 1) * width,
                    height - normalized * (height - 4d) - 2d);

                linePoints.Add(point);
                areaPoints.Add(point);
            }

            areaPoints.Add(new Point(width, height));
            areaPoints.Add(new Point(0, height));

            SpeedPolyline.Points = linePoints;
            AreaPolygon.Points = areaPoints;
        }

        private void ChartHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RebuildChart();
        }

        private void DashboardSlide_Loaded(object sender, RoutedEventArgs e)
        {
            _frameTimer.Start();
        }

        private void DashboardSlide_Unloaded(object sender, RoutedEventArgs e)
        {
            _frameTimer.Stop();
        }
    }
}
