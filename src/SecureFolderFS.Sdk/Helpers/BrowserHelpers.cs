using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class BrowserHelpers
    {
        public static BrowserViewModel CreateBrowser(UnlockedVaultViewModel unlockedVaultViewModel, INavigator? innerNavigator = null, INavigator? outerNavigator = null)
        {
            innerNavigator ??= DI.Service<INavigationService>();
            var rootFolder = unlockedVaultViewModel.StorageRoot.VirtualizedRoot;
            var browserViewModel = new BrowserViewModel(rootFolder, innerNavigator, outerNavigator, unlockedVaultViewModel.VaultViewModel);
            var transferViewModel = new TransferViewModel(browserViewModel);
            var folderViewModel = new FolderViewModel(rootFolder, browserViewModel, null);
            _ = folderViewModel.ListContentsAsync();
            
            browserViewModel.TransferViewModel = transferViewModel;
            browserViewModel.CurrentFolder = folderViewModel;

            return browserViewModel;
        }
    }
}
