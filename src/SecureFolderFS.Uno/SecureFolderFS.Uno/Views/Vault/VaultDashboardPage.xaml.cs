using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultDashboardPage : Page
    {
        public VaultDashboardPageViewModel ViewModel
        {
            get => (VaultDashboardPageViewModel)DataContext;
            set { DataContext = value; OnPropertyChanged(); }
        }

        public VaultDashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultDashboardPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Remove the reference to the NavigationControl so the page can get properly garbage collected
            ViewModel.DashboardNavigationService.ResetNavigation();
            ViewModel.DashboardNavigationService.NavigationChanged -= DashboardNavigationService_NavigationChanged;
            Navigation.Dispose();

            base.OnNavigatingFrom(e);
        }

        private async void Navigation_Loaded(object sender, RoutedEventArgs e)
        {
            // Hook up navigation event
            ViewModel.DashboardNavigationService.NavigationChanged += DashboardNavigationService_NavigationChanged;

            // Initialize navigation
            if (ViewModel.DashboardNavigationService.SetupNavigation(Navigation, true))
            {
                var target = ViewModel.DashboardNavigationService.CurrentTarget ?? GetDefaultDashboardViewModel(); // Get current target or initialize default
                INavigationTarget GetDefaultDashboardViewModel()
                {
                    var controlsViewModel = new VaultControlsViewModel(ViewModel.UnlockedVaultViewModel, ViewModel.DashboardNavigationService, ViewModel.NavigationService);
                    return new VaultOverviewPageViewModel(ViewModel.UnlockedVaultViewModel, controlsViewModel, ViewModel.DashboardNavigationService);
                }

                await ViewModel.DashboardNavigationService.NavigateAsync(target);
            }
        }

        private async void DashboardNavigationService_NavigationChanged(object? sender, INavigationTarget? e)
        {
            var canGoBack = e switch
            {
                VaultOverviewPageViewModel => false,
                _ => true
            };

            if (canGoBack)
            {
                GoBack.Visibility = Visibility.Visible;
                await ShowBackButtonStoryboard.BeginAsync();
                ShowBackButtonStoryboard.Stop();
            }
            else
            {
                await HideBackButtonStoryboard.BeginAsync();
                HideBackButtonStoryboard.Stop();
                GoBack.Visibility = Visibility.Collapsed;
            }
        }
    }
}
