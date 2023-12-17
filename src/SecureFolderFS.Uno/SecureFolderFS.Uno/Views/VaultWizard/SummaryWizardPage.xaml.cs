using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class SummaryWizardPage : Page, IDisposable
    {
        public SummaryWizardViewModel ViewModel
        {
            get => (SummaryWizardViewModel)DataContext;
            set { DataContext = value; OnPropertyChanged(); }
        }

        public SummaryWizardPage()
        {
            InitializeComponent();
            UnoThemeHelper.Instance.PropertyChanged += ThemeHelper_PropertyChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SummaryWizardViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void ThemeHelper_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IThemeHelper.CurrentTheme))
                return;

            CheckVisualSource.SetColorProperty("Foreground", ((SolidColorBrush)Application.Current.Resources["SolidBackgroundFillColorBaseBrush"]).Color);
        }

        private async void VisualPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            CheckVisualSource.SetColorProperty("Foreground", ((SolidColorBrush)Application.Current.Resources["SolidBackgroundFillColorBaseBrush"]).Color);
            VisualPlayer.Visibility = Visibility.Collapsed;

            await Task.Delay(600);
            _ = VisualPlayer.PlayAsync(CheckVisualSource.Markers["NormalOffToNormalOn_Start"], CheckVisualSource.Markers["NormalOffToNormalOn_End"], false);
            await Task.Delay(20);

            VisualPlayer.Visibility = Visibility.Visible;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            UnoThemeHelper.Instance.PropertyChanged -= ThemeHelper_PropertyChanged; 
        }
    }
}
