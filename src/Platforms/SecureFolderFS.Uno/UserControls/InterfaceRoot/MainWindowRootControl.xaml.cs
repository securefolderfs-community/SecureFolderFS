using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.InterfaceRoot
{
    public sealed partial class MainWindowRootControl : UserControl
    {
        private INavigationService RootNavigationService { get; } = DI.Service<INavigationService>();

        public MainViewModel? ViewModel
        {
            get => DataContext.TryCast<MainViewModel>();
            set => DataContext = value;
        }

        public MainWindowRootControl()
        {
            InitializeComponent();
            ViewModel = new();
        }

        private void MainWindowRootControl_Loaded(object sender, RoutedEventArgs e)
        {
            RootNavigationService.SetupNavigation(Navigation);
            _ = EnsureRootAsync();
        }

        private async Task EnsureRootAsync()
        {
#if WINDOWS
            // Small delay for Mica material to load
            await Task.Delay(1);
#endif
            // Initialize ThemeHelper for theming
            UnoThemeHelper.Instance.RegisterWindowInstance(App.Instance?.MainWindow?.Content as FrameworkElement);
            await UnoThemeHelper.Instance.InitAsync();

            if (ViewModel is null)
                return;

            // Initialize the root view model
            await ViewModel.InitAsync();
            if (!ViewModel.VaultCollectionModel.IsEmpty()) // Has vaults
            {
                // Show main app screen
                await RootNavigationService.TryNavigateAsync(() => new MainHostViewModel(ViewModel.VaultCollectionModel), false);
            }
            else // Doesn't have vaults
            {
                // Show no vaults screen
                await RootNavigationService.TryNavigateAsync(() => new EmptyHostViewModel(RootNavigationService, ViewModel.VaultCollectionModel), false);
            }
        }
    }
}
