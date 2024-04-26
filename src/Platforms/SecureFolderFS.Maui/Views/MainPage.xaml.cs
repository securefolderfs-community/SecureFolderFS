using SecureFolderFS.Maui.UserControls.Navigation;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
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

            var target = ViewModel.NavigationService.Views.FirstOrDefault(x => (x as BaseVaultPageViewModel)?.VaultViewModel == itemViewModel.VaultViewModel);
            if (target is null)
            {
                target = new VaultLoginPageViewModel(itemViewModel.VaultViewModel, ViewModel.NavigationService);
                if (target is IAsyncInitialize asyncInitialize)
                    await asyncInitialize.InitAsync();
            }

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }
    }
}
