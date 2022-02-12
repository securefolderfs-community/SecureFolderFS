using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using SecureFolderFS.Backend.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class GraphWidgetControl : UserControl, IDisposable
    {
        public GraphWidgetControl()
        {
            this.InitializeComponent();
        }

        private async void RootButton_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);
            _ = FindName("CartesianChart"); // Realize the chart and load it to view
            await Task.Delay(100);
            ControlLoaded = true;
        }

        public event RoutedEventHandler Click;

        public bool ControlLoaded
        {
            get => (bool)GetValue(ControlLoadedProperty);
            set => SetValue(ControlLoadedProperty, value);
        }
        public static readonly DependencyProperty ControlLoadedProperty =
            DependencyProperty.Register("ControlLoaded", typeof(bool), typeof(GraphWidgetControl), new PropertyMetadata(false));


        public ObservableCollection<GraphPointModel> Data
        {
            get => (ObservableCollection<GraphPointModel>)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(ObservableCollection<GraphPointModel>), typeof(GraphWidgetControl), new PropertyMetadata(null));


        public string GraphHeader
        {
            get => (string)GetValue(GraphHeaderProperty);
            set => SetValue(GraphHeaderProperty, value);
        }
        public static readonly DependencyProperty GraphHeaderProperty =
            DependencyProperty.Register("GraphHeader", typeof(string), typeof(GraphWidgetControl), new PropertyMetadata(null));


        public string GraphSubheader
        {
            get => (string)GetValue(GraphSubheaderProperty);
            set => SetValue(GraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty GraphSubheaderProperty =
            DependencyProperty.Register("GraphSubheader", typeof(string), typeof(GraphWidgetControl), new PropertyMetadata(null));


        public Brush ChartStroke
        {
            get => (Brush)GetValue(ChartStrokeProperty);
            set => SetValue(ChartStrokeProperty, value);
        }
        public static readonly DependencyProperty ChartStrokeProperty =
            DependencyProperty.Register("ChartStroke", typeof(Brush), typeof(GraphWidgetControl), new PropertyMetadata(null));


        public Color ChartPrimaryColor
        {
            get => (Color)GetValue(ChartPrimaryColorProperty);
            set => SetValue(ChartPrimaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartPrimaryColorProperty =
            DependencyProperty.Register("ChartPrimaryColor", typeof(Color), typeof(GraphWidgetControl), new PropertyMetadata(null));


        public Color ChartSecondaryColor
        {
            get => (Color)GetValue(ChartSecondaryColorProperty);
            set => SetValue(ChartSecondaryColorProperty, value);
        }
        public static readonly DependencyProperty ChartSecondaryColorProperty =
            DependencyProperty.Register("ChartSecondaryColor", typeof(Color), typeof(GraphWidgetControl), new PropertyMetadata(null));


        public void Dispose()
        {
            CartesianChart?.Dispose();
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }
    }
}
