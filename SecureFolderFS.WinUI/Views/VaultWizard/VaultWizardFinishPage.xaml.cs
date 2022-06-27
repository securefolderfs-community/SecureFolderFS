using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.WindowViews;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultWizardFinishPage : Page, IDisposable
    {
        public VaultWizardSummaryViewModel ViewModel
        {
            get => (VaultWizardSummaryViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultWizardFinishPage()
        {
            InitializeComponent();

            ThemeHelper.ThemeHelpers[MainWindow.Instance!.AppWindow!].RegisterForThemeChangedCallback(nameof(VaultWizardFinishPage), _ =>
            {
                CheckVisualSource.SetColorProperty("Foreground", ((SolidColorBrush)Application.Current.Resources["SolidBackgroundFillColorBaseBrush"]).Color);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardSummaryViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            base.OnNavigatedTo(e);
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

        public void Dispose()
        {
            ThemeHelper.ThemeHelpers[MainWindow.Instance!.AppWindow!].UnregisterForThemeChangedCallback(nameof(VaultWizardFinishPage));
            ViewModel.Dispose();
        }
    }
}
