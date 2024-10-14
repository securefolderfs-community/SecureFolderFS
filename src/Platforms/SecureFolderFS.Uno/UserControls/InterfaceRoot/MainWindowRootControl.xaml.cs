using System;
using System.Diagnostics;
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
        public INavigationService RootNavigationService { get; } = DI.Service<INavigationService>();

        public bool IsDebugging { get; } =
#if DEBUG
            Debugger.IsAttached;
#else
            false;
#endif

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
            if (OperatingSystem.IsMacCatalyst())
                RootGrid.Margin = new(0, 37, 0, 0);

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
            UnoThemeHelper.Instance.RegisterWindowInstance(App.Instance?.MainWindow?.Content as FrameworkElement, App.Instance?.MainWindow?.AppWindow);

            if (ViewModel is null)
                return;

            // Initialize the root view model
            await ViewModel.InitAsync();

            // Initialize theme
            await UnoThemeHelper.Instance.InitAsync();

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

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var window = new Window()
            {
                Content = new DebugWindowRootControl(),
                Title = $"{nameof(SecureFolderFS)} Debug Window",
            };
            window.AppWindow?.MoveAndResize(new(100, 100, 700, 900));

            global::Uno.UI.WindowExtensions.EnableHotReload(window);
            window.Activate();
#endif
        }
    }
}
