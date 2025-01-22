using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
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
        private bool _isLoaded;
        
        public VaultDashboardViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultDashboardViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public VaultDashboardPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultDashboardViewModel viewModel)
            {
                ViewModel = viewModel;
                await LoadComponentsAsync();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ViewModel is not null)
            {
                // Remove the reference to the NavigationControl so the page can get properly garbage collected
                ViewModel.DashboardNavigation.ResetNavigation();
                ViewModel.DashboardNavigation.NavigationChanged -= DashboardNavigation_NavigationChanged;
            }
            
            Navigation.Dispose();
            base.OnNavigatingFrom(e);
        }

        private async Task LoadComponentsAsync()
        {
            if (_isLoaded || ViewModel is null)
                return;
            
            // Update _isLoaded flag
            _isLoaded = true;
            
            // Attach navigation event
            ViewModel.DashboardNavigation.NavigationChanged += DashboardNavigation_NavigationChanged;

            // Initialize navigation
            if (ViewModel.DashboardNavigation.SetupNavigation(Navigation, true))
            {
                var target = ViewModel.DashboardNavigation.CurrentView ?? GetDefaultDashboardViewModel(); // Get current target or initialize default
                await ViewModel.DashboardNavigation.NavigateAsync(target);

                IViewDesignation GetDefaultDashboardViewModel()
                {
                    var vaultOverviewViewModel = new VaultOverviewViewModel(
                        ViewModel.UnlockedVaultViewModel,
                        new(ViewModel.VaultNavigation, ViewModel.DashboardNavigation, ViewModel.UnlockedVaultViewModel),
                        new(ViewModel.UnlockedVaultViewModel, ViewModel.DashboardNavigation, new WidgetsCollectionModel(ViewModel.VaultViewModel.VaultModel.Folder)));

                    _ = vaultOverviewViewModel.InitAsync();
                    return vaultOverviewViewModel;
                }
            }
        }

        private async void Navigation_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadComponentsAsync();
        }

        private async void DashboardNavigation_NavigationChanged(object? sender, IViewDesignation? e)
        {
            var canGoBack = e is not VaultOverviewViewModel;
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
