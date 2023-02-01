using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.AvaloniaUI.Services;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.AppHost;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceRoot
{
    public sealed partial class MainWindowRootControl : UserControl, IRecipient<RootNavigationRequestedMessage>
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

        public void Receive(RootNavigationRequestedMessage message)
        {
            _ = NavigateHostControlAsync(message.ViewModel);
        }



        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindowRootControl_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _ = EnsureRootAsync();
            WeakReferenceMessenger.Default.Register(this);
        }

        private async Task EnsureRootAsync()
        {
            var vaultCollectionModel = new LocalVaultCollectionModel();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            var applicationSettingsService = Ioc.Default.GetRequiredService<IApplicationSettingsService>();
            var platformSettingsService = Ioc.Default.GetRequiredService<IPlatformSettingsService>();

            // Initialize
            var result = await Task.WhenAll(
                applicationSettingsService.LoadSettingsAsync(),
                settingsService.LoadSettingsAsync(),
                platformSettingsService.LoadSettingsAsync(),
                vaultCollectionModel.HasVaultsAsync());

            ThemeHelper.Instance.UpdateTheme();

            // Continue root initialization
            if (false && applicationSettingsService.IsIntroduced) // TODO: Always skipped
            {
                //ViewModel.AppContentViewModel = new MainAppHostViewModel(vaultCollectionModel);
                // TODO: Implement OOBE
            }
            else
            {
                if (result[3]) // Has vaults
                {
                    // Show main app screen
                    _ = NavigateHostControlAsync(new MainAppHostViewModel(vaultCollectionModel));
                }
                else // Doesn't have vaults
                {
                    // Show no vaults screen
                    _ = NavigateHostControlAsync(new NoVaultsAppHostViewModel(vaultCollectionModel));
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