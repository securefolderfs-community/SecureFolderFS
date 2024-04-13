using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultLoginPage : Page
    {
        public VaultLoginViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultLoginViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public VaultLoginPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultLoginViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.NavigationRequested -= ViewModel_NavigationRequested;
                ViewModel.NavigationRequested += ViewModel_NavigationRequested;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ViewModel is not null)
                ViewModel.NavigationRequested -= ViewModel_NavigationRequested;

            base.OnNavigatingFrom(e);
        }

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (e is not UnlockNavigationRequestedEventArgs args)
                return;

            ViewModel.NavigationRequested -= ViewModel_NavigationRequested;

            var dashboardViewModel = new VaultDashboardViewModel(args.UnlockedVaultViewModel, ViewModel.VaultNavigator);
            _ = dashboardViewModel.InitAsync();

            if (ViewModel.VaultNavigator is INavigationService navigationService)
                await navigationService.TryNavigateAndForgetAsync(dashboardViewModel);
            else
                await ViewModel.VaultNavigator.NavigateAsync(dashboardViewModel);
        }
    }
}
