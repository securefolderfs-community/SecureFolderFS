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

            var result = string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);
            return _isAscending ? result : -result;
        }
    }
}
