using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;

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
            if (e.Item is not VaultListItemViewModel itemViewModel)
                return;

            await Shell.Current.GoToAsync("LoginPage", new VaultLoginPageViewModel(itemViewModel.VaultViewModel, null).ViewModelParameter());
        }

        private void AddButton_Clicked(object? sender, EventArgs e)
        {
            var vaultViewModel = new VaultViewModel(new VaultModel(null, "TestVaultName"), null);
            var sidebarItem = new VaultListItemViewModel(vaultViewModel, Shell.Current.TryCast<AppShell>()!.MainViewModel.VaultCollectionModel);

            ViewModel.VaultListViewModel.Items.Add(sidebarItem);
        }
    }
}
