using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Brush = System.Drawing.Brush;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    internal sealed partial class GraphControl : UserControl
    {
        public event EventHandler<RoutedEventArgs>? Click;

        public GraphControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Chart_Loaded(object sender, RoutedEventArgs e)
        {
            Chart.Series = new ISeries[]
            {
                new LineSeries<DateTimePoint>
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
                new Axis
                {
                    Labeler = x => string.Empty,
                    ShowSeparatorLines = false
                }
            };
            Chart.YAxes = new ICartesianAxis[]
            {
                new Axis
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
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly StyledProperty<IList?> DataProperty =
            AvaloniaProperty.Register<GraphControl, IList?>(nameof(Data));

        public string? GraphHeader
        {
            get => GetValue(GraphHeaderProperty);
            set => SetValue(GraphHeaderProperty, value);
        }
        public static readonly StyledProperty<string?> GraphHeaderProperty =
            AvaloniaProperty.Register<GraphControl, string?>(nameof(GraphHeader));

        public string? GraphSubheader
        {
            get => GetValue(GraphSubheaderProperty);
            set => SetValue(GraphSubheaderProperty, value);
        }
        public static readonly StyledProperty<string?> GraphSubheaderProperty =
            AvaloniaProperty.Register<GraphControl, string?>(nameof(GraphSubheader));

        public Color ChartStrokeColor
        {
            get => GetValue(ChartStrokeColorProperty);
            set => SetValue(ChartStrokeColorProperty, value);
        }
        public static readonly StyledProperty<Color> ChartStrokeColorProperty =
            AvaloniaProperty.Register<GraphControl, Color>(nameof(ChartStrokeColor));

        public Color ChartPrimaryColor
        {
            get => GetValue(ChartPrimaryColorProperty);
            set => SetValue(ChartPrimaryColorProperty, value);
        }
        public static readonly StyledProperty<Color> ChartPrimaryColorProperty =
            AvaloniaProperty.Register<GraphControl, Color>(nameof(ChartPrimaryColor));

        public Color ChartSecondaryColor
        {
            get => GetValue(ChartSecondaryColorProperty);
            set => SetValue(ChartSecondaryColorProperty, value);
        }
        public static readonly StyledProperty<Color> ChartSecondaryColorProperty =
            AvaloniaProperty.Register<GraphControl, Color>(nameof(ChartSecondaryColor));

        public bool GraphLoaded
        {
            get => (bool)GetValue(GraphLoadedProperty);
            private set => SetValue(GraphLoadedProperty, value);
        }
        public static readonly StyledProperty<bool> GraphLoadedProperty =
            AvaloniaProperty.Register<GraphControl, bool>(nameof(GraphLoaded));
    }
}