using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.AppHost;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.InterfaceRoot
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

        /// <inheritdoc/>
        public void Receive(RootNavigationRequestedMessage message)
        {
            _ = NavigateHostControlAsync(message.ViewModel);
        }

        private void MainWindowRootControl_Loaded(object sender, RoutedEventArgs e)
        {
            _ = EnsureRootAsync();
            WeakReferenceMessenger.Default.Register(this);
        }

        private async Task EnsureRootAsync()
        {
            var vaultCollectionModel = new LocalVaultCollectionModel();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

            // Small delay for Mica material to load
            await Task.Delay(1);

            // Initialize
            var result = await Task.WhenAll(settingsService.InitAsync(), vaultCollectionModel.HasVaultsAsync());

            // Continue root initialization
            if (false && settingsService.ApplicationSettings.IsIntroduced) // TODO: Always skipped
            {
                //ViewModel.AppContentViewModel = new MainAppHostViewModel(vaultCollectionModel);
                // TODO: Implement OOBE
            }
            else
            {
                if (result[2]) // Has vaults
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
            if ((ViewModel.AppContentViewModel is null && viewModel is not MainAppHostViewModel)
                || (ViewModel.AppContentViewModel is not MainAppHostViewModel && ViewModel.AppContentViewModel is not null && viewModel is MainAppHostViewModel))
                AppContent?.ContentTransitions?.ClearAndAdd(new ContentThemeTransition());

            ViewModel.AppContentViewModel = viewModel;

            await Task.Delay(250);
            AppContent?.ContentTransitions?.Clear();
        }
    }
}
