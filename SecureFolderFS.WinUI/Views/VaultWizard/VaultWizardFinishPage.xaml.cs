using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.Windows;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultWizardFinishPage : Page, IDisposable
    {
        public VaultWizardFinishPageViewModel ViewModel
        {
            get => (VaultWizardFinishPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultWizardFinishPage()
        {
            this.InitializeComponent();

            ThemeHelper.ThemeHelpers[MainWindow.Instance!.AppWindow!].RegisterForThemeChangeCallback(nameof(VaultWizardFinishPage), _ =>
            {
                CheckVisualSource.SetColorProperty("Foreground", ((SolidColorBrush)Application.Current.Resources["SolidBackgroundFillColorBaseBrush"]).Color);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardFinishPageViewModel viewModel)
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
            ThemeHelper.ThemeHelpers[MainWindow.Instance!.AppWindow!].UnregisterForThemeChangeCallback(nameof(VaultWizardFinishPage));
            ViewModel.Dispose();
        }
    }
}
