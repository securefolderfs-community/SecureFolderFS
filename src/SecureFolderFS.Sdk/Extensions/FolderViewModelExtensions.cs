using System.Collections.Generic;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class FolderViewModelExtensions
    {
        public static void SelectAll(this FolderViewModel folderViewModel)
        {
            if (!folderViewModel.BrowserViewModel.IsSelecting)
                return;
            
            foreach (var item in folderViewModel.Items)
            {
                item.IsSelected = true;
            }
        }

        public static void UnselectAll(this FolderViewModel folderViewModel)
        {
            foreach (var item in folderViewModel.Items)
            {
                item.IsSelected = false;
            }
        }

        public static IEnumerable<BrowserItemViewModel> GetSelectedItems(this FolderViewModel folderViewModel)
        {
            if (!folderViewModel.BrowserViewModel.IsSelecting)
                yield break;
            
            foreach (var item in folderViewModel.Items)
            {
                if (item.IsSelected)
                    yield return item;
            }
        }
    }
}