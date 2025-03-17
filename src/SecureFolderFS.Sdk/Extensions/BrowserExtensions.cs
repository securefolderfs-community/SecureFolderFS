using System;
using System.Collections.ObjectModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class BrowserExtensions
    {
        public static void InsertSorted(this FolderViewModel folderViewModel, BrowserItemViewModel itemViewModel)
        {
            var insertIndex = -1;

            // Folders first, then alphabetical by name
            for (var i = 0; i < folderViewModel.Items.Count; i++)
            {
                var currentItem = folderViewModel.Items[i];
                var isNewItemFolder = itemViewModel is FolderViewModel;
                var isCurrentItemFolder = currentItem is FolderViewModel;

                // Insert if we reach a folder after a file or a smaller alphabetical value
                if ((isNewItemFolder && !isCurrentItemFolder)
                    || (isNewItemFolder == isCurrentItemFolder && string.Compare(itemViewModel.Title, currentItem.Title, StringComparison.Ordinal) < 0))
                {
                    insertIndex = Math.Min(Math.Max(i - 1, 0), folderViewModel.Items.Count);
                    break;
                }
            }

            // If no match, insert at the end
            if (insertIndex == -1)
                folderViewModel.Items.Add(itemViewModel);
            else
                folderViewModel.Items.Insert(insertIndex, itemViewModel);
        }
    }
}
