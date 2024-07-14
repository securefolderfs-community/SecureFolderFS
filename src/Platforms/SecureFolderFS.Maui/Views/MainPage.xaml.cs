using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.UserControls.Navigation;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Views
{
    public partial class MainPage : ContentPage
    {
        public MainHostViewModel ViewModel { get; } = new(Shell.Current.TryCast<AppShell>()!.MainViewModel.VaultCollectionModel);

        public static MainPage? Instance { get; private set; }

        public MainPage()
        {
            Instance = this;
            BindingContext = this;
            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000

            InitializeComponent();
        }

        private async void ListView_ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            ViewModel.NavigationService.SetupNavigation(ShellNavigationControl.Instance);
            if (e.Item is not VaultListItemViewModel itemViewModel)
                return;

            var target = ViewModel.NavigationService.Views.FirstOrDefault(x => (x as BaseVaultViewModel)?.VaultModel.Equals(itemViewModel.VaultModel) ?? false);
            if (target is null)
            {
                var vaultLoginViewModel = new VaultLoginViewModel(itemViewModel.VaultModel, ViewModel.NavigationService);
                _ = vaultLoginViewModel.InitAsync();
                target = vaultLoginViewModel;
            }

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            // Need to set Title here because MainPage is instantiated before services are configured
            Title = "MyVaults".ToLocalized();

            // Also set the current starting view
            if (ViewModel.NavigationService is MauiNavigationService navigationService)
                navigationService.SetCurrentViewInternal(ViewModel);
        }
    }
}
