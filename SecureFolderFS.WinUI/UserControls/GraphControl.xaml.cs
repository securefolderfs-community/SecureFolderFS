using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class GraphControl : UserControl
    {
        public event RoutedEventHandler? Click;

        public GraphControl()
        {
            InitializeComponent();
        }

        private async void RootButton_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(25);
            try
            {
                // Realize the chart and load it to view
                _ = FindName(nameof(Chart));
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        private async void Chart_Loaded(object sender, RoutedEventArgs e)
        {
            Chart.Series = new ISeries[]
            {
                new LineSeries<DateTimePoint>()
                {
                    Values = new DateTimePoint[] { },
                    Fill = new LinearGradientPaint(
                        new(ChartPrimaryColor.R, ChartPrimaryColor.G, ChartPrimaryColor.B, ChartPrimaryColor.A),
                        new(ChartSecondaryColor.R, ChartSecondaryColor.G, ChartSecondaryColor.B, ChartSecondaryColor.A),
                        new(0.5f, 0f), new(0.5f, 1.0f)),
                    GeometrySize = 0d,
                    Stroke = new SolidColorPaint(new(ChartStrokeColor.R, ChartStrokeColor.G, ChartStrokeColor.B, ChartStrokeColor.A), 2)
                }
            };
            Chart.XAxes = new ICartesianAxis[]
            {
                new Axis()
                {
                    Labeler = x => string.Empty,
                    ShowSeparatorLines = false
                }
            };
            Chart.YAxes = new ICartesianAxis[]
            {
                new Axis()
                {
                    ShowSeparatorLines = false,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                }
            };

            await Task.Delay(25);
            GraphLoaded = true;
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        public IList? Data
        {
            get => (IList?)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(IList), typeof(GraphControl), new PropertyMetadata(null));

        public string? GraphHeader
        {
            get => (string?)GetValue(GraphHeaderProperty);
            set => SetValue(GraphHeaderProperty, value);
        }
        public static readonly DependencyProperty GraphHeaderProperty =
            DependencyProperty.Register(nameof(GraphHeader), typeof(string), typeof(GraphControl), new PropertyMetadata(null));

        public string? GraphSubheader
        {
            get => (string?)GetValue(GraphSubheaderProperty);
            set => SetValue(GraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty GraphSubheaderProperty =
            DependencyProperty.Register(nameof(GraphSubheader), typeof(string), typeof(GraphControl), new PropertyMetadata(null));

        public Color ChartStrokeColor
        {
            get => (Color)GetValue(ChartStrokeColorProperty);
            set => SetValue(ChartStrokeColorProperty, value);
        }
        public static readonly DependencyProperty ChartStrokeColorProperty =
            DependencyProperty.Register(nameof(ChartStrokeColor), typeof(Brush), typeof(GraphControl), new PropertyMetadata(default));

        public Color ChartPrimaryColor
        {
            get => (Color)GetValue(ChartPrimaryColorProperty);
            set => SetValue(ChartPrimaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartPrimaryColorProperty =
            DependencyProperty.Register(nameof(ChartPrimaryColor), typeof(Color), typeof(GraphControl), new PropertyMetadata(default));

        public Color ChartSecondaryColor
        {
            get => (Color)GetValue(ChartSecondaryColorProperty);
            set => SetValue(ChartSecondaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartSecondaryColorProperty =
            DependencyProperty.Register(nameof(ChartSecondaryColor), typeof(Color), typeof(GraphControl), new PropertyMetadata(default));

        public bool GraphLoaded
        {
            get => (bool)GetValue(GraphLoadedProperty);
            private set => SetValue(GraphLoadedProperty, value);
        }
        public static readonly DependencyProperty GraphLoadedProperty =
            DependencyProperty.Register(nameof(GraphLoaded), typeof(bool), typeof(GraphControl), new PropertyMetadata(false));
    }
}
