using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.Backend.ViewModels.Dashboard;
using Windows.UI;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class GraphWidgetControl : UserControl
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
            ViewModel.GraphLoaded = true;
        }


        public GraphWidgetControlViewModel ViewModel
        {
            get => (GraphWidgetControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(GraphWidgetControlViewModel), typeof(GraphWidgetControl), new PropertyMetadata(null));


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


        public string GraphHeader
        {
            get => (string)GetValue(GraphHeaderProperty);
            set => SetValue(GraphHeaderProperty, value);
        }

        public static readonly DependencyProperty GraphHeaderProperty =
            DependencyProperty.Register("GraphHeader", typeof(string), typeof(GraphWidgetControl), new PropertyMetadata(null));
    }
}
