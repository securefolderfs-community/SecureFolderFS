using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class GraphControl : UserControl, IDisposable
    {
        public event RoutedEventHandler? Click;

        public GraphControl()
        {
            InitializeComponent();
        }

        private async void RootButton_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);
            _ = FindName("CartesianChart"); // Realize the chart and load it to view
            await Task.Delay(25);
            ControlLoaded = true;
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        public void Dispose()
        {
            CartesianChart?.Dispose();
        }

        public bool ControlLoaded
        {
            get => (bool)GetValue(ControlLoadedProperty);
            set => SetValue(ControlLoadedProperty, value);
        }
        public static readonly DependencyProperty ControlLoadedProperty =
            DependencyProperty.Register(nameof(ControlLoaded), typeof(bool), typeof(GraphControl), new PropertyMetadata(false));


        public ObservableCollection<GraphPointModel> Data
        {
            get => (ObservableCollection<GraphPointModel>)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(ObservableCollection<GraphPointModel>), typeof(GraphControl), new PropertyMetadata(null));


        public string GraphHeader
        {
            get => (string)GetValue(GraphHeaderProperty);
            set => SetValue(GraphHeaderProperty, value);
        }
        public static readonly DependencyProperty GraphHeaderProperty =
            DependencyProperty.Register(nameof(GraphHeader), typeof(string), typeof(GraphControl), new PropertyMetadata(null));


        public string GraphSubheader
        {
            get => (string)GetValue(GraphSubheaderProperty);
            set => SetValue(GraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty GraphSubheaderProperty =
            DependencyProperty.Register(nameof(GraphSubheader), typeof(string), typeof(GraphControl), new PropertyMetadata(null));


        public Brush ChartStroke
        {
            get => (Brush)GetValue(ChartStrokeProperty);
            set => SetValue(ChartStrokeProperty, value);
        }
        public static readonly DependencyProperty ChartStrokeProperty =
            DependencyProperty.Register(nameof(ChartStroke), typeof(Brush), typeof(GraphControl), new PropertyMetadata(null));


        public Color ChartPrimaryColor
        {
            get => (Color)GetValue(ChartPrimaryColorProperty);
            set => SetValue(ChartPrimaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartPrimaryColorProperty =
            DependencyProperty.Register(nameof(ChartPrimaryColor), typeof(Color), typeof(GraphControl), new PropertyMetadata(null));


        public Color ChartSecondaryColor
        {
            get => (Color)GetValue(ChartSecondaryColorProperty);
            set => SetValue(ChartSecondaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartSecondaryColorProperty =
            DependencyProperty.Register(nameof(ChartSecondaryColor), typeof(Color), typeof(GraphControl), new PropertyMetadata(null));
    }
}
