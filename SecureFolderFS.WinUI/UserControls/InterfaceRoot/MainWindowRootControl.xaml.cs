using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.WindowViews;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.InterfaceRoot
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
            InitializeComponent();

            ViewModel = new();
        }

        private void MainWindowRootControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.HostNavigationService.SetupNavigation(Navigation);
            _ = EnsureRootAsync();
        }

        private async Task EnsureRootAsync()
        {
            // Small delay for Mica material to load
            await Task.Delay(1);

            // Initialize the data in the background
            _ = ViewModel.InitAsync();

            // First register the ThemeHelper
            WindowsThemeHelper.Instance.RegisterWindowInstance(MainWindow.Instance.AppWindow, MainWindow.Instance.RootControl);

            // Then, initialize it to refresh the theme and UI
            await WindowsThemeHelper.Instance.InitAsync();
        }
    }
}
