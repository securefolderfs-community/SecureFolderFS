using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.Uno.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class GeneralSettingsPage : Page
    {
        private bool _isFirstTime = true;

        public GeneralSettingsViewModel? ViewModel
        {
            get => DataContext.TryCast<GeneralSettingsViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public int SelectedThemeIndex => (int)UnoThemeHelper.Instance.CurrentTheme;

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

        private async void AppThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
                return;
            }

            await UnoThemeHelper.Instance.SetThemeAsync((ThemeType)AppThemeComboBox.SelectedIndex);
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            _ = AddTransitionsAsync();
            async Task AddTransitionsAsync()
            {
                // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
                await Task.Delay(400);
                (sender as Panel)?.ChildrenTransitions?.Add(new AddDeleteThemeTransition());
            }
        }
    }
}
