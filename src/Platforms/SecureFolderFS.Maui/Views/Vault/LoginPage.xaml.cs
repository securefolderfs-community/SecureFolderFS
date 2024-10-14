using MauiIcons.Core;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class LoginPage : ContentPageExtended, IQueryAttributable
    {
        public LoginPage()
        {
            BindingContext = this;
            _ = new MauiIcon(); // Workaround for XFC0000

            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<VaultLoginViewModel>();
            if (ViewModel is not null)
            {
                ViewModel.NavigationRequested -= ViewModel_NavigationRequested;
                ViewModel.NavigationRequested += ViewModel_NavigationRequested;
            }

            OnPropertyChanged(nameof(ViewModel));
        }

        protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
        {
            if (ViewModel is not null)
                ViewModel.NavigationRequested -= ViewModel_NavigationRequested;

            base.OnNavigatingFrom(args);
        }

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (e is not UnlockNavigationRequestedEventArgs args)
                return;

            ViewModel.NavigationRequested -= ViewModel_NavigationRequested;

            // Initialize DashboardViewModel and use the same navigation for dashboard
            var dashboardNavigation = DI.Service<INavigationService>();
            var dashboardViewModel = new VaultDashboardViewModel(args.UnlockedVaultViewModel, ViewModel.VaultNavigation, dashboardNavigation);
            
            // Since both overview and properties are on the same page,
            // initialize and navigate the views to keep them in cache

            var propertiesViewModel = new VaultPropertiesViewModel(args.UnlockedVaultViewModel);
            var overviewViewModel = new VaultOverviewViewModel(
                args.UnlockedVaultViewModel,
                new(ViewModel.VaultNavigation, ViewModel.VaultNavigation, args.UnlockedVaultViewModel, propertiesViewModel),
                new(args.UnlockedVaultViewModel,
                    new WidgetsCollectionModel(args.UnlockedVaultViewModel.VaultViewModel.VaultModel.Folder)));
            
            // Set Title to 'fake' navigation
            dashboardViewModel.Title = overviewViewModel.Title;

            // Initialize both properties and overview
            _ = Task.Run(async () =>
            {
                await propertiesViewModel.InitAsync();
                await overviewViewModel.InitAsync();
            });
            
            // Persist view models
            dashboardViewModel.DashboardNavigation.Views.Add(overviewViewModel);
            dashboardViewModel.DashboardNavigation.Views.Add(propertiesViewModel);

            await ViewModel.VaultNavigation.TryNavigateAndForgetAsync(dashboardViewModel);
        }

        public VaultLoginViewModel? ViewModel
        {
            get => (VaultLoginViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultLoginViewModel), typeof(LoginPage), null);
    }
}
