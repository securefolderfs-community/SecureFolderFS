using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.WinUI.Helpers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SummaryWizardPage : Page, IDisposable
    {
        public VaultWizardSummaryViewModel ViewModel
        {
            get => (VaultWizardSummaryViewModel)DataContext;
            set => DataContext = value;
        }

        public SummaryWizardPage()
        {
            InitializeComponent();
            WinUIThemeHelper.Instance.PropertyChanged += ThemeHelper_PropertyChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardSummaryViewModel viewModel)
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
            WinUIThemeHelper.Instance.PropertyChanged -= ThemeHelper_PropertyChanged;
        }
    }
}
