using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeneralSettingsPage : Page
    {
        public GeneralSettingsPageViewModel ViewModel
        {
            get => (GeneralSettingsPageViewModel)DataContext;
            set => DataContext = value;
        }

        public GeneralSettingsPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is GeneralSettingsPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.BannerViewModel.ConfigureUpdates();
        }

        private void AppLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox { SelectedItem: ILanguageModel language })
                ViewModel.LanguageSettingViewModel.UpdateCurrentLanguage(language);
        }

        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
            await Task.Delay(400);
            RootGrid?.ChildrenTransitions?.Add(new ReorderThemeTransition());
        }
    }
}
