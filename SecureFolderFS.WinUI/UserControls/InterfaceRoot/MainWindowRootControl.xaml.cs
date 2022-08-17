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
using SecureFolderFS.Sdk.Services;

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

        private async void MainWindowRootControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vaultCollectionModel = new LocalVaultCollectionModel();
            var applicationSettingsService = Ioc.Default.GetRequiredService<IApplicationSettingsService>();
            var threadingService = Ioc.Default.GetRequiredService<IThreadingService>();

            // Small delay for Mica material to load
            await Task.Delay(1);

            // Continue root initialization
            _ = applicationSettingsService.LoadSettingsAsync()
                .ContinueWith(_ =>
                {
                    // Check if user was introduced
                    // TODO: Implement introduction page view model
                    //if (applicationSettingsService.IsIntroduced)
                    //    ViewModel.AppContentViewModel = new AppViewModel(vaultCollectionModel);
                })
                .ContinueWith(async _ =>
                {
                    await threadingService.ExecuteOnUiThreadAsync();

                    // Determine which app screen to show
                    if (ViewModel.AppContentViewModel is null)
                    {
                        if (await vaultCollectionModel.HasVaultsAsync())
                        {
                            // Show main app screen
                            ViewModel.AppContentViewModel = new MainAppHostViewModel(vaultCollectionModel);
                        }
                        else
                        {
                            // Show no vaults screen
                            ViewModel.AppContentViewModel = new NoVaultsAppHostViewModel(vaultCollectionModel);
                        }
                    }
                }).Unwrap();

            WeakReferenceMessenger.Default.Register(this);
        }
    }
}
