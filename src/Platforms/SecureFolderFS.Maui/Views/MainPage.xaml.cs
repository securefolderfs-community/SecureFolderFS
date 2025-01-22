using MauiIcons.Core;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.UserControls.Navigation;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Views
{
    public partial class MainPage : ContentPageExtended
    {
        public MainHostViewModel ViewModel { get; } = new(Shell.Current.TryCast<AppShell>()!.MainViewModel.VaultCollectionModel);

        public static MainPage? Instance { get; private set; }

        public MainPage()
        {
            Instance = this;
            BindingContext = this;
            _ = ViewModel.InitAsync();
            _ = new MauiIcon(); // Workaround for XFC0000

            InitializeComponent();
        }

        private async void ListView_ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            ViewModel.NavigationService.SetupNavigation(ShellNavigationControl.Instance);
            if (e.Item is not VaultListItemViewModel itemViewModel)
                return;

            var target = ViewModel.NavigationService.Views.FirstOrDefault(x => (x as IVaultViewContext)?.VaultViewModel.VaultModel.Equals(itemViewModel.VaultViewModel.VaultModel) ?? false);
            if (target is null)
            {
                var vaultLoginViewModel = new VaultLoginViewModel(itemViewModel.VaultViewModel, ViewModel.NavigationService);
                _ = vaultLoginViewModel.InitAsync();
                target = vaultLoginViewModel;
            }

            // Navigate
            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private async void MainPage_Loaded(object? sender, EventArgs e)
        {
            // Set the current starting view
            if (ViewModel.NavigationService.CurrentView is null && ViewModel.NavigationService is MauiNavigationService navigationService)
                navigationService.SetCurrentViewInternal(ViewModel);

#if IOS
            if (!ExToolbarItems.IsEmpty())
                return;
            
            ExToolbarItems.Add(new ExMenuItem()
            {
                Text = "NewVault".ToLocalized(),
                Command = ViewModel.VaultListViewModel.AddNewVaultCommand,
                Order = ToolbarItemOrder.Secondary
            });
            ExToolbarItems.Add(new ExMenuItem()
            {
                Text = "Settings".ToLocalized(),
                Command = ViewModel.OpenSettingsCommand,
                Order = ToolbarItemOrder.Secondary
            });
#endif
        }
    }
}
