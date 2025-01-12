using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
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
            // Workaround for the application freezing after unlocking at least 2 vaults
            // TODO: Find the cause of the issue and fix it
            await Task.Delay(500);

            if (sender is not CartesianChart chart)
                return;

            chart.Series =
            [
                new LineSeries<double>()
                {
                    Values = Data,
                    Fill = new LinearGradientPaint([
                            new(ChartPrimaryColor.R, ChartPrimaryColor.G, ChartPrimaryColor.B, ChartPrimaryColor.A),
                            new(ChartSecondaryColor.R, ChartSecondaryColor.G, ChartSecondaryColor.B, ChartSecondaryColor.A)
                        ],
                        new(0.5f, 0f), new(0.5f, 1.0f),[0.2f, 1.3f]),
                    Stroke = new SolidColorPaint(new(ChartStrokeColor.R, ChartStrokeColor.G, ChartStrokeColor.B, ChartStrokeColor.A), 2),
                    LineSmoothness = 0d,
                    DataPadding = new(0.5f, 0),
                    AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                    IsHoverable = false,
                    GeometrySize = 0d // TODO: Setting this to 0 causes a bug with jumping line series
                }
            ];
            chart.XAxes =
            [
                new Axis()
                {
                    Labeler = x => string.Empty,
                    ShowSeparatorLines = false
                }
            ];
            chart.YAxes =
            [
                new Axis()
                {
                    ShowSeparatorLines = false,
                    Padding = new Padding(16, 0, 0, 0),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    MinLimit = 0d
                }
            ];

            await Task.Delay(25);
            GraphLoaded = true;
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        public ICollection<double>? Data
        {
            get => (ICollection<double>?)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(ICollection<double>), typeof(GraphControl), new PropertyMetadata(defaultValue: null));

        public string? GraphHeader
        {
            get => (string?)GetValue(GraphHeaderProperty);
            set => SetValue(GraphHeaderProperty, value);
        }
        public static readonly DependencyProperty GraphHeaderProperty =
            DependencyProperty.Register(nameof(GraphHeader), typeof(string), typeof(GraphControl), new PropertyMetadata(defaultValue: null));

        public string? GraphSubheader
        {
            get => (string?)GetValue(GraphSubheaderProperty);
            set => SetValue(GraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty GraphSubheaderProperty =
            DependencyProperty.Register(nameof(GraphSubheader), typeof(string), typeof(GraphControl), new PropertyMetadata(defaultValue: null));

        public Color ChartStrokeColor
        {
            get => (Color)GetValue(ChartStrokeColorProperty);
            set => SetValue(ChartStrokeColorProperty, value);
        }
        public static readonly DependencyProperty ChartStrokeColorProperty =
            DependencyProperty.Register(nameof(ChartStrokeColor), typeof(Brush), typeof(GraphControl), new PropertyMetadata(defaultValue: default));

        public Color ChartPrimaryColor
        {
            get => (Color)GetValue(ChartPrimaryColorProperty);
            set => SetValue(ChartPrimaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartPrimaryColorProperty =
            DependencyProperty.Register(nameof(ChartPrimaryColor), typeof(Color), typeof(GraphControl), new PropertyMetadata(defaultValue: default));

        public Color ChartSecondaryColor
        {
            get => (Color)GetValue(ChartSecondaryColorProperty);
            set => SetValue(ChartSecondaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartSecondaryColorProperty =
            DependencyProperty.Register(nameof(ChartSecondaryColor), typeof(Color), typeof(GraphControl), new PropertyMetadata(defaultValue: default));

        public bool GraphLoaded
        {
            get => (bool)GetValue(GraphLoadedProperty);
            private set => SetValue(GraphLoadedProperty, value);
        }
        public static readonly DependencyProperty GraphLoadedProperty =
            DependencyProperty.Register(nameof(GraphLoaded), typeof(bool), typeof(GraphControl), new PropertyMetadata(defaultValue: false));
    }
}
