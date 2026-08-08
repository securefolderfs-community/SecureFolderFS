using System;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.AppModels.Sorters
{
    public sealed class DateSorter : BaseFolderSorter
    {
        private readonly bool _isAscending;

        public static IItemSorter<BrowserItemViewModel> Ascending { get; } = new DateSorter(true);

        public static IItemSorter<BrowserItemViewModel> Descending { get; } = new DateSorter(false);

        private DateSorter(bool isAscending)
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

            var result = DateTime.Compare(x.LastModified ?? DateTime.MinValue, y.LastModified ?? DateTime.MinValue);
            return _isAscending ? result : -result;
        }
    }
}
