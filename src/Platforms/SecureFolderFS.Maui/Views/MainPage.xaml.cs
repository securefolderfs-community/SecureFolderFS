using SecureFolderFS.Maui.UserControls.Navigation;
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

        public MainPage()
        {
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
    }
}
