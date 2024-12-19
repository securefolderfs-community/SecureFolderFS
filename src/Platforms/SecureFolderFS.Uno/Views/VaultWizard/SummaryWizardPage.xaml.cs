using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Uno.Helpers;

#if WINDOWS
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.UI.Helpers;
#endif

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class SummaryWizardPage : Page
    {
        public SummaryWizardViewModel? ViewModel
        {
            get => DataContext.TryCast<SummaryWizardViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public SummaryWizardPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SummaryWizardViewModel viewModel)
                ViewModel = viewModel;

            UnoThemeHelper.Instance.PropertyChanged += ThemeHelper_PropertyChanged;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            UnoThemeHelper.Instance.PropertyChanged -= ThemeHelper_PropertyChanged;
            base.OnNavigatingFrom(e);
        }

        private void ThemeHelper_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
#if WINDOWS
            if (e.PropertyName != nameof(ThemeHelper.CurrentTheme))
                return;

            CheckVisualSource.SetColorProperty("Foreground", ((SolidColorBrush)Application.Current.Resources["SolidBackgroundFillColorBaseBrush"]).Color);
#endif
        }

        private async void VisualPlayer_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS
            CheckVisualSource.SetColorProperty("Foreground", ((SolidColorBrush)Application.Current.Resources["SolidBackgroundFillColorBaseBrush"]).Color);
            VisualPlayer.Visibility = Visibility.Collapsed;

            await Task.Delay(600);
            _ = VisualPlayer.PlayAsync(CheckVisualSource.Markers["NormalOffToNormalOn_Start"], CheckVisualSource.Markers["NormalOffToNormalOn_End"], false);
            await Task.Delay(20);

            VisualPlayer.Visibility = Visibility.Visible;
#else
            await Task.CompletedTask;
#endif
        }
    }
}
