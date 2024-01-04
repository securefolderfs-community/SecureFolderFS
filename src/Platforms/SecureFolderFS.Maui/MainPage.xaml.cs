using System.Collections.ObjectModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;

namespace SecureFolderFS.Maui
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

        private void ListView_ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            _ = e;
        }
    }
}
