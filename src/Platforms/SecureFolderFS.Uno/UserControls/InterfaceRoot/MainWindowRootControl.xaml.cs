using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
    [INotifyPropertyChanged]
    public sealed partial class MainWindowRootControl : UserControl
    {
        public INavigationService RootNavigationService { get; } = DI.Service<INavigationService>();

        public SynchronizationContext? Context { get; }

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
            Context = SynchronizationContext.Current;
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
#if !__IOS__
                Title = $"{nameof(SecureFolderFS)} Debug Window",
#endif
            };
#if WINDOWS
            window.AppWindow?.MoveAndResize(new(100, 100, 700, 900));
#endif

            global::Uno.UI.WindowExtensions.EnableHotReload(window);
            window.Activate();
#endif
        }

        [RelayCommand]
        private void TaskbarIconDoubleClick()
        {
            App.Instance?.MainWindow?.Activate();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (App.Instance is null)
                return;

            App.Instance.UseForceClose = true;
            Application.Current.Exit();
        }
    }
}
