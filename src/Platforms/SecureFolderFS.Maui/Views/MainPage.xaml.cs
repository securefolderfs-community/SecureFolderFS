using SecureFolderFS.Maui.UserControls.Navigation;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
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
                var vaultLoginViewModel = new VaultLoginViewModel(itemViewModel.VaultModel);
                vaultLoginViewModel.NavigationRequested += VaultLoginViewModel_NavigationRequested;

                target = vaultLoginViewModel;
                if (target is IAsyncInitialize asyncInitialize)
                    await asyncInitialize.InitAsync();
            }

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private async void VaultLoginViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (sender is BaseVaultViewModel viewModel)
                viewModel.NavigationRequested -= VaultLoginViewModel_NavigationRequested;

            if (e is UnlockNavigationRequestedEventArgs args)
            {
                var vaultOverviewViewModel = new VaultOverviewViewModel(
                    args.UnlockedVaultViewModel,
                    new(ViewModel.NavigationService, args.UnlockedVaultViewModel),
                    new(args.UnlockedVaultViewModel, new WidgetsCollectionModel(args.UnlockedVaultViewModel.VaultModel.Folder)));

                _ = vaultOverviewViewModel.InitAsync();
                await ViewModel.NavigationService.NavigateAsync(vaultOverviewViewModel);
            }
        }
    }
}
