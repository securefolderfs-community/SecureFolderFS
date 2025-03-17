using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.AppModels.Sorters
{
    public abstract class BaseFolderSorter : IItemSorter<BrowserItemViewModel>
    {
        private readonly Lock _lock = new();

        /// <inheritdoc/>
        public virtual int GetInsertIndex(BrowserItemViewModel newItem, ICollection<BrowserItemViewModel> collection)
        {
            var low = 0;
            var high = collection.Count;

            while (low < high)
            {
                var mid = (low + high) / 2;

                // If newItem should come before the mid-element, search the left half.
                if (Compare(newItem, collection.ElementAt(mid)) < 0)
                    high = mid;
                else
                    low = mid + 1;
            }

            return low;
        }

        /// <inheritdoc/>
        public virtual void SortCollection(IEnumerable<BrowserItemViewModel> source, ICollection<BrowserItemViewModel> destination)
        {
            var sortedList = new List<BrowserItemViewModel>(source);
            sortedList.Sort(Compare);
            destination.Clear();
            foreach (var item in sortedList)
            {
                destination.Add(item);
            }
        }

        /// <inheritdoc/>
        public abstract int Compare(BrowserItemViewModel? x, BrowserItemViewModel? y);
    }
}
