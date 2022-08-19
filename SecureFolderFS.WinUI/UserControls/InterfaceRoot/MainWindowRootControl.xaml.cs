using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.AppHost;
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
            ViewModel.AppContentViewModel = message.ViewModel as ObservableObject;
        }

        private void MainWindowRootControl_Loaded(object sender, RoutedEventArgs e)
        {
            _ = EnsureRootAsync();

            WeakReferenceMessenger.Default.Register(this);
        }

        private async Task EnsureRootAsync()
        {
            var vaultCollectionModel = new LocalVaultCollectionModel();
            var applicationSettingsService = Ioc.Default.GetRequiredService<IApplicationSettingsService>();

            // Small delay for Mica material to load
            await Task.Delay(1);

            // Initialize
            var result = await Task.WhenAll(applicationSettingsService.LoadSettingsAsync(), vaultCollectionModel.HasVaultsAsync());

            // Continue root initialization
            if (false && applicationSettingsService.IsIntroduced) // TODO: Always skipped
            {
                ViewModel.AppContentViewModel = new MainAppHostViewModel(vaultCollectionModel);
            }
            else
            {
                if (result[1]) // Has vaults
                {
                    // Show main app screen
                    ViewModel.AppContentViewModel = new MainAppHostViewModel(vaultCollectionModel);
                }
                else // Doesn't have vaults
                {
                    // Show no vaults screen
                    ViewModel.AppContentViewModel = new NoVaultsAppHostViewModel(vaultCollectionModel);
                }
            }
        }
    }
}
