using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.UserControls.BreadcrumbBar;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultDashboardPage : Page
    {
        public ObservableCollection<OrderedBreadcrumbBarItem> BreadcrumbItems { get; }

        public VaultDashboardPageViewModel ViewModel
        {
            get => (VaultDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultDashboardPage()
        {
            InitializeComponent();
            BreadcrumbItems = new();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultDashboardPageViewModel viewModel)
                ViewModel = viewModel;

            BreadcrumbItems.Add(new(ViewModel.VaultViewModel.VaultModel.VaultName, true));
        }

        private async void Navigation_Loaded(object sender, RoutedEventArgs e)
        {
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
    }
}
