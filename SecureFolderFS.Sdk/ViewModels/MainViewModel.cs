using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages;
using SecureFolderFS.Sdk.ViewModels.Sidebar;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        public IVaultCollectionModel VaultCollection { get; }

        public SidebarViewModel SidebarViewModel { get; }

        public SavedVaultsModel SavedVaultsModel { get; }

        public BasePageViewModel? CurrentPageViewModel { get; set; }

        public MainViewModel()
        {
            VaultCollection = new LocalVaultCollectionModel();
            SidebarViewModel = new();
            SavedVaultsModel = new()
            {
                InitializableSource = SidebarViewModel
            };
        }
    }
}
