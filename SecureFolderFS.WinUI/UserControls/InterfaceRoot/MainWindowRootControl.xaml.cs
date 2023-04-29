using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.WindowViews;
using System.ComponentModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.InterfaceRoot
{
    public sealed partial class MainWindowRootControl : UserControl, IRecipient<RootNavigationMessage>
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
        public void Receive(RootNavigationMessage message)
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
            var vaultCollectionModel = new VaultCollectionModel();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

            // Small delay for Mica material to load
            await Task.Delay(1);

            // Initialize
            await Task.WhenAll(settingsService.LoadAsync(), vaultCollectionModel.LoadAsync());

            // First register the ThemeHelper
            WindowsThemeHelper.Instance.RegisterWindowInstance(MainWindow.Instance.AppWindow, MainWindow.Instance.HostControl);

            // Then, initialize it to refresh the theme and UI
            await WindowsThemeHelper.Instance.InitAsync();
             
            // Continue root initialization
            if (false && settingsService.AppSettings.IsIntroduced) // TODO: Always skipped
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
            if ((ViewModel.AppContentViewModel is null && viewModel is not MainHostViewModel)
                || (ViewModel.AppContentViewModel is not MainHostViewModel &&
                    ViewModel.AppContentViewModel is not null && viewModel is MainHostViewModel))
            {
                AppContent?.ContentTransitions?.Clear();
                AppContent?.ContentTransitions?.Add(new ContentThemeTransition());
            }

            ViewModel.AppContentViewModel = viewModel;

            await Task.Delay(250);
            AppContent?.ContentTransitions?.Clear();
        }
    }
}
