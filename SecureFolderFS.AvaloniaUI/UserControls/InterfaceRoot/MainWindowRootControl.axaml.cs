using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.Sdk.ViewModels;
using System.Threading.Tasks;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceRoot
{
    public sealed partial class MainWindowRootControl : UserControl
    {
        public MainViewModel ViewModel
        {
            get => (MainViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindowRootControl()
        {
            AvaloniaXamlLoader.Load(this);

            ViewModel = new();
        }

        private void MainWindowRootControl_Loaded(object? sender, RoutedEventArgs e)
        {
            ViewModel.HostNavigationService.SetupNavigation(Navigation);
            _ = EnsureRootAsync();
        }

        private async Task EnsureRootAsync()
        {
            // Initialize the data in the background
            _ = ViewModel.InitAsync();
            
            // TODO Register ThemeHelper

            // Update UI to reflect the current theme
            await AvaloniaThemeHelper.Instance.InitAsync();
        }
    }
}