using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Root;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.Helpers;
using Uno.UI;
#if WINDOWS
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
#endif

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.InterfaceRoot
{
    public sealed partial class MainWindowRootControl : UserControl
    {
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

        public MainWindowRootControl(MainViewModel mainViewModel)
        {
            InitializeComponent();
            App.Instance.MainWindowSynchronizationContext = SynchronizationContext.Current;
            ViewModel = mainViewModel;
        }

        private void MainWindowRootControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (OperatingSystem.IsMacCatalyst())
                RootGrid.Margin = new(0, 37, 0, 0);

            ViewModel.RootNavigationService.SetupNavigation(Navigation);
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

            if (!VaultListMigratorHelpers.IsMigrated())
            {
                var file = await VaultListMigratorHelpers.TryGetVaultsFileAsync(CancellationToken.None);
                if (file is not null)
                {
                    await VaultListMigratorHelpers.TryMigrateVaultsAsync(file, StreamSerializer.Instance, CancellationToken.None);
                    VaultListMigratorHelpers.SetMigrated();
                }
            }

            // Initialize the root view model
            await ViewModel.InitAsync();

            // Initialize theme
            await UnoThemeHelper.Instance.InitAsync();

            if (!ViewModel.VaultCollectionModel.IsEmpty()) // Has vaults
            {
                // Show main app screen
                await ViewModel.RootNavigationService.TryNavigateAsync(() => new MainHostViewModel(ViewModel.VaultListViewModel, ViewModel.VaultCollectionModel), false);
            }
            else // Doesn't have vaults
            {
                // Show no vaults screen
                await ViewModel.RootNavigationService.TryNavigateAsync(() => new EmptyHostViewModel(ViewModel.VaultListViewModel, ViewModel.RootNavigationService, ViewModel.VaultCollectionModel), false);
            }
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var window = new Window()
            {
                Content = new DebugWindowRootControl(),
#if !__IOS__
                Title = $"{nameof(SecureFolderFS)} Debug Window",
#endif
            };
#if WINDOWS
            window.AppWindow?.MoveAndResize(new(100, 100, 700, 900));
#endif
            window.EnableHotReload();
            window.Activate();
#endif
        }

        [RelayCommand]
        private void TaskbarIconDoubleClick()
        {
            App.Instance?.MainWindow?.Activate();
        }

        private void MenuCloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (App.Instance is null)
                return;

            App.Instance.UseForceClose = true;
            Application.Current.Exit();
        }

        private void MenuShowApp_Click(object sender, RoutedEventArgs e)
        {
            App.Instance?.MainWindow?.Activate();
        }

        private void MenuLockAll_Click(object sender, RoutedEventArgs e)
        {
#if WINDOWS
            if (ViewModel is null)
                return;

            foreach (var item in ViewModel.VaultCollectionModel)
                WeakReferenceMessenger.Default.Send(new VaultLockRequestedMessage(item));
#endif
        }
    }
}
