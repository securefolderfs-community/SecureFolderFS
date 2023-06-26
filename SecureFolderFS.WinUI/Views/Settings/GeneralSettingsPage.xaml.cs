using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.WinUI.Helpers;
using System.Globalization;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeneralSettingsPage : Page
    {
        private bool _isFirstTime = true;

        public GeneralSettingsViewModel ViewModel
        {
            get => (GeneralSettingsViewModel)DataContext;
            set => DataContext = value;
        }

        public int SelectedThemeIndex => (int)WindowsThemeHelper.Instance.CurrentTheme;

        public GeneralSettingsPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is GeneralSettingsViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private async void AppLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppLanguageComboBox.SelectedItem is not CultureInfo cultureInfo)
                return;

            await ViewModel.LocalizationService.SetCultureAsync(cultureInfo);
        }

        private async void AppThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
                return;
            }

            await WindowsThemeHelper.Instance.SetThemeAsync((ThemeType)AppThemeComboBox.SelectedIndex);
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
                await Task.Delay(400);
                RootGrid?.ChildrenTransitions?.Add(new ReorderThemeTransition());
            });
        }
    }
}
