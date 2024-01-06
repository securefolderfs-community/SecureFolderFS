using System.Collections.ObjectModel;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;

namespace SecureFolderFS.Maui.Views
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<VaultListItemViewModel> Vaults { get; } = new ObservableCollection<VaultListItemViewModel>();

        public MainPage()
        {
            BindingContext = this;
            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000

            InitializeComponent();

            Vaults.Add(new(new(new VaultModel(null, "TestVault", new()), null), null));
            Vaults.Add(new(new(new VaultModel(null, "TestVault2"), null), null));
        }

        private async void ListView_ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            var vaultVM = new VaultViewModel(new VaultModel(null, "abc"), null);
            await Shell.Current.GoToAsync("LoginPage", new VaultLoginPageViewModel(vaultVM, null).ViewModelParameter());
            _ = e;
        }
    }
}
