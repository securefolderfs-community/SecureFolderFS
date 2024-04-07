using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class DashboardPage : ContentPage, IQueryAttributable
    {
        public DashboardPage()
        {
            BindingContext = this;
            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000

            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<VaultDashboardPageViewModel>()!;
            OnPropertyChanged(nameof(ViewModel));
        }

        protected async override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            await LoadComponentsAsync();
        }

        private async Task LoadComponentsAsync()
        {
            if (ViewModel is null)
                return;

            // Initialize navigation
            if (ViewModel.DashboardNavigationService.SetupNavigation(DashboardNavigation, true))
            {
                var target = ViewModel.DashboardNavigationService.CurrentView ?? GetDefaultDashboardViewModel(); // Get current target or initialize default
                await ViewModel.DashboardNavigationService.NavigateAsync(target);

                IViewDesignation GetDefaultDashboardViewModel()
                {
                    var controlsViewModel = new VaultControlsViewModel(ViewModel.UnlockedVaultViewModel, ViewModel.DashboardNavigationService, ViewModel.NavigationService);
                    var vaultOverviewViewModel = new VaultOverviewPageViewModel(ViewModel.UnlockedVaultViewModel, controlsViewModel, ViewModel.DashboardNavigationService);
                    _ = vaultOverviewViewModel.InitAsync();

                    return vaultOverviewViewModel;
                }
            }
        }

        public VaultDashboardPageViewModel? ViewModel
        {
            get => (VaultDashboardPageViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultDashboardPageViewModel), typeof(DashboardPage), null);
    }
}
