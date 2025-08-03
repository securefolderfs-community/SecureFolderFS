using System;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class BrowserHelpers
    {
        [Obsolete]
        public static BrowserViewModel CreateBrowser(IFolder rootFolder, FileSystemOptions options, UnlockedVaultViewModel unlockedVaultViewModel, INavigator? innerNavigator = null, INavigator? outerNavigator = null)
        {
            innerNavigator ??= DI.Service<INavigationService>();
            var browserViewModel = new BrowserViewModel(rootFolder, options, innerNavigator, outerNavigator, unlockedVaultViewModel.VaultViewModel)
            {
                StorageRoot = unlockedVaultViewModel.StorageRoot
            };
            var transferViewModel = new TransferViewModel(browserViewModel);
            var folderViewModel = new FolderViewModel(rootFolder, browserViewModel, null);

            browserViewModel.TransferViewModel = transferViewModel;
            browserViewModel.CurrentFolder = folderViewModel;

            return browserViewModel;
        }

        public static BrowserViewModel CreateBrowser(IFolder rootFolder, FileSystemOptions options, IViewable? rootView, INavigator? innerNavigator = null, INavigator? outerNavigator = null)
        {
            innerNavigator ??= DI.Service<INavigationService>();
            var browserViewModel = new BrowserViewModel(rootFolder, options, innerNavigator, outerNavigator, rootView);
            var transferViewModel = new TransferViewModel(browserViewModel);
            var folderViewModel = new FolderViewModel(rootFolder, browserViewModel, null);

            browserViewModel.TransferViewModel = transferViewModel;
            browserViewModel.CurrentFolder = folderViewModel;

            return browserViewModel;
        }
    }
}
