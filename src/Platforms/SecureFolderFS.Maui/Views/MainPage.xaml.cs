#if ANDROID
using CustomToolbarItem = Microsoft.Maui.Controls.ToolbarItem;
#elif IOS
using CustomToolbarItem = SecureFolderFS.Maui.AppModels.ExMenuItem;
#endif
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.UserControls.Navigation;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Extensions;
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

            InitializeComponent();
        }

        private async Task ItemTappedAsync(View? view, VaultListItemViewModel itemViewModel)
        {
            if (view is not null)
                view.IsEnabled = false;

            try
            {
                ViewModel.NavigationService.SetupNavigation(ShellNavigationControl.Instance);
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
            finally
            {
                if (view is not null)
                    view.IsEnabled = true;
            }
        }

        private async void ListView_ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            if (sender is not View view)
                return;

            await ItemTappedAsync(view, (VaultListItemViewModel)e.Item);
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            // Set the current starting view
            if (ViewModel.NavigationService.CurrentView is null && ViewModel.NavigationService is MauiNavigationService navigationService)
                navigationService.SetCurrentViewInternal(ViewModel);

#if IOS
            var toolbarItems = ExToolbarItems;
#elif ANDROID
            var toolbarItems = ToolbarItems;
#endif
            if (!toolbarItems.IsEmpty())
                return;

#if IOS
            toolbarItems.Add(new CustomToolbarItem()
            {
                Text = "NewVault".ToLocalized(),
                Command = ViewModel.VaultListViewModel.AddNewVaultCommand,
                Order = ToolbarItemOrder.Secondary
            });
#endif
            toolbarItems.Add(new CustomToolbarItem()
            {
                Text = "Settings".ToLocalized(),
                Command = ViewModel.OpenSettingsCommand,
                Order = ToolbarItemOrder.Secondary
            });
        }
    }
}
