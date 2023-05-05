using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SecureFolderFS.AvaloniaUI.Messages;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    internal sealed partial class GraphControl : UserControl, IRecipient<DialogShownMessage>, IRecipient<DialogHiddenMessage>
    {
        public event EventHandler<RoutedEventArgs>? Click;

        public GraphControl()
        {
            AvaloniaXamlLoader.Load(this);
            WeakReferenceMessenger.Default.Register<DialogShownMessage>(this);
            WeakReferenceMessenger.Default.Register<DialogHiddenMessage>(this);
        }

        private async void Chart_Loaded(object sender, RoutedEventArgs e)
        {
            // Workaround for the application freezing after unlocking at least 2 vaults
            // TODO Find the cause of the issue and fix it
            await Task.Delay(500);

            Chart.Series = new ISeries[]
            {
                new LineSeries<GraphPoint>()
                {
                    Values = Data,
                    Fill = new LinearGradientPaint(new SKColor[] {
                            new(ChartPrimaryColor.R, ChartPrimaryColor.G, ChartPrimaryColor.B, ChartPrimaryColor.A),
                            new(ChartSecondaryColor.R, ChartSecondaryColor.G, ChartSecondaryColor.B, ChartSecondaryColor.A) },
                        new(0.5f, 0f), new(0.5f, 1.0f), new[] { 0.2f, 1.3f }),
                    GeometrySize = 0d,
                    Stroke = new SolidColorPaint(new(ChartStrokeColor.R, ChartStrokeColor.G, ChartStrokeColor.B, ChartStrokeColor.A), 2),
                    Mapping = (model, point) => { point.PrimaryValue = Convert.ToDouble(model.Value); point.SecondaryValue = model.Date.Ticks; },
                    LineSmoothness = 0d,
                    DataPadding = new(0.3f, 0),
                    AnimationsSpeed = TimeSpan.FromMilliseconds(150),
                    GeometryStroke = new SolidColorPaint(SKColors.Transparent),
                    IsHoverable = false,
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
                    Padding = new Padding(16, 0, 0, 0),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    MinLimit = 0d
                }
            };

            await Task.Delay(25);
            GraphLoaded = true;
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        // Fix major graphical glitches when a dialog and a chart are visible at the same time
        // Disabling deferred rendering does solve this, but it causes invisible elements to appear
        // on the screen, which prevent the user from pressing buttons.
        public void Receive(DialogShownMessage message)
        {
            Chart.IsVisible = false;
        }

        public async void Receive(DialogHiddenMessage message)
        {
            await Task.Delay(250); // Ensure the dialog is closed
            Chart.IsVisible = true;
        }

        public IList<GraphPoint>? Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly StyledProperty<IList<GraphPoint>?> DataProperty =
            AvaloniaProperty.Register<GraphControl, IList<GraphPoint>?>(nameof(Data));

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
            get => GetValue(GraphLoadedProperty);
            private set => SetValue(GraphLoadedProperty, value);
        }
        public static readonly StyledProperty<bool> GraphLoadedProperty =
            AvaloniaProperty.Register<GraphControl, bool>(nameof(GraphLoaded));
    }
}