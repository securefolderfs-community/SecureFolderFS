using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceRoot
{
    public sealed partial class MainWindowRootControl : UserControl, IRecipient<RootNavigationMessage>
    {
        public MainViewModel? ViewModel
        {
            get => (MainViewModel?)DataContext;
            set => DataContext = value;
        }

        public MainWindowRootControl()
        {
            AvaloniaXamlLoader.Load(this);

            ViewModel = new();
        }

        public void Receive(RootNavigationMessage message)
        {
            _ = NavigateHostControlAsync(message.ViewModel);
        }

        private void MainWindowRootControl_Loaded(object? sender, RoutedEventArgs e)
        {
            _ = EnsureRootAsync();
            WeakReferenceMessenger.Default.Register(this);
        }

        private async Task EnsureRootAsync()
        {
            var vaultCollectionModel = new VaultCollectionModel();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            var telemetryService = Ioc.Default.GetRequiredService<ITelemetryService>();

            // Initialize
            await Task.WhenAll(settingsService.LoadAsync(), vaultCollectionModel.LoadAsync());

            // Update UI to reflect the current theme
            await AvaloniaThemeHelper.Instance.InitAsync();

            // Disable telemetry, if the user opted-out
            if (!settingsService.UserSettings.IsTelemetryEnabled)
                await telemetryService.DisableTelemetryAsync();

            // Continue root initialization
            if (false && !settingsService.AppSettings.IsIntroduced) // TODO: Always skipped
            {
                //ViewModel.AppContentViewModel = new MainAppHostViewModel(vaultCollectionModel);
                // TODO: Implement OOBE
            }
            else
            {
                if (!vaultCollectionModel.GetVaults().IsEmpty()) // Has vaults
                {
                    // Show main app screen
                    _ = NavigateHostControlAsync(new MainHostViewModel(vaultCollectionModel)); // TODO(r)
                }
                else // Doesn't have vaults
                {
                    // Show no vaults screen
                    _ = NavigateHostControlAsync(new EmptyHostViewModel(vaultCollectionModel));
                }
            }
        }

        private async Task NavigateHostControlAsync(INotifyPropertyChanged viewModel)
        {
            // Use transitions only when the initial page view model is not MainAppHostViewModel
            /*
            if ((ViewModel.AppContentViewModel is null && viewModel is not MainAppHostViewModel)
                || (ViewModel.AppContentViewModel is not MainAppHostViewModel && ViewModel.AppContentViewModel is not null && viewModel is MainAppHostViewModel))
                AppContent?.ContentTransitions?.ClearAndAdd(new ContentThemeTransition());
                */
            // TODO transitions

            ViewModel.AppContentViewModel = viewModel;

            //await Task.Delay(250);
            //AppContent?.ContentTransitions?.Clear();
        }
    }
}