using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.AppModels.Sorters
{
    public sealed class SizeSorter : BaseFolderSorter
    {
        private readonly bool _isAscending;
        
        public static IItemSorter<BrowserItemViewModel> Ascending { get; } = new SizeSorter(true);
        
        public static IItemSorter<BrowserItemViewModel> Descending { get; } = new SizeSorter(false);

        private SizeSorter(bool isAscending)
        {
            _isAscending = isAscending;
        }

        /// <inheritdoc/>
        public override int Compare(BrowserItemViewModel? x, BrowserItemViewModel? y)
        {
            if (x is null || y is null)
                return 0;

            var size1 = GetSizeAsync(x).ConfigureAwait(false).GetAwaiter().GetResult();
            var size2 = GetSizeAsync(y).ConfigureAwait(false).GetAwaiter().GetResult();
                
            var result = size1.CompareTo(size2);
            return SortDirection == SortDirection.Ascending ? result : -result;
        }
        
        private static async Task<long> GetSizeAsync(BrowserItemViewModel browserItemViewModel)
        {
            if (browserItemViewModel is not FileViewModel fileViewModel)
                return -1L;

            if (fileViewModel.File is not IStorableProperties storableProperties)
                return 0L;

            var properties = await storableProperties.GetPropertiesAsync().ConfigureAwait(false);
            if (properties is not ISizeProperties sizeProperties)
                return 0L;

            var sizeProperty = await sizeProperties.GetSizeAsync().ConfigureAwait(false);
            if (sizeProperty is null)
                return 0L;
            
            return sizeProperty.Value;
        }
    }
}
