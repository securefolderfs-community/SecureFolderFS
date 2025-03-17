using System;
using System.IO;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.AppModels.Sorters
{
    public sealed class KindSorter : BaseFolderSorter
    {
        private readonly bool _isAscending;
        
        public static IItemSorter<BrowserItemViewModel> Ascending { get; } = new KindSorter(true);

        public static IItemSorter<BrowserItemViewModel> Descending { get; } = new KindSorter(false);

        private KindSorter(bool isAscending)
        {
            _isAscending = isAscending;
        }
        
        /// <inheritdoc/>
        public override int Compare(BrowserItemViewModel? x, BrowserItemViewModel? y)
        {
            if (x is null || y is null)
                return 0;
            
            var result = string.Compare(GetKind(x), GetKind(y), StringComparison.OrdinalIgnoreCase);
            return _isAscending ? result : -result;
        }
        
        private static string GetKind(BrowserItemViewModel item)
        {
            switch (item)
            {
                case FolderViewModel: return string.Empty;
                case FileViewModel:
                {
                    var extension = Path.GetExtension(item.Inner.Name);
                    return string.IsNullOrEmpty(extension) ? "File" : extension;
                }
                
                default: return "Unknown";
            }
        }
    }
}
