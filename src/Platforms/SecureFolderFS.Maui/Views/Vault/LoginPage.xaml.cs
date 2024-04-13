using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class LoginPage : ContentPage, IQueryAttributable
    {
        public LoginPage()
        {
            BindingContext = this;
            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000

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

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (e is not UnlockNavigationRequestedEventArgs args)
                return;

            ViewModel.NavigationRequested -= ViewModel_NavigationRequested;

            var vaultOverviewViewModel = new VaultOverviewViewModel(
                args.UnlockedVaultViewModel,
                new(ViewModel.VaultNavigator, ViewModel.VaultNavigator, args.UnlockedVaultViewModel),
                new(args.UnlockedVaultViewModel, new WidgetsCollectionModel(args.UnlockedVaultViewModel.VaultModel.Folder)));

            _ = vaultOverviewViewModel.InitAsync();
            if (ViewModel.VaultNavigator is INavigationService navigationService)
                await navigationService.TryNavigateAndForgetAsync(vaultOverviewViewModel);
            else
                await ViewModel.VaultNavigator.NavigateAsync(vaultOverviewViewModel);
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
