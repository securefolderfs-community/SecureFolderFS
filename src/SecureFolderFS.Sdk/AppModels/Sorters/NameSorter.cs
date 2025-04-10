using System;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.AppModels.Sorters
{
    public sealed class NameSorter : BaseFolderSorter
    {
        private readonly bool _isAscending;

        public static IItemSorter<BrowserItemViewModel> Ascending { get; } = new NameSorter(true);

        public static IItemSorter<BrowserItemViewModel> Descending { get; } = new NameSorter(false);

        private NameSorter(bool isAscending)
        {
            _isAscending = isAscending;
        }

        /// <inheritdoc/>
        public override int Compare(BrowserItemViewModel? x, BrowserItemViewModel? y)
        {
            if (x is null || y is null)
                return 0;

            // Ensure folders come before files
            var xIsFolder = x is FolderViewModel;
            var yIsFolder = y is FolderViewModel;

            if (xIsFolder && !yIsFolder)
                return -1;
            
            if (!xIsFolder && yIsFolder)
                return 1;

            // If both are same type, sort by name
            var result = string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);
            return _isAscending ? result : -result;
        }
    }
}
