using System.Collections.Generic;

namespace SecureFolderFS.Shared.ComponentModel
{
    public interface IItemSorter<T> : IComparer<T>
    {
        /// <summary>
        /// Determines the correct index for inserting a new item so that the collection remains sorted.
        /// Uses a binary search for efficiency.
        /// </summary>
        /// <param name="newItem">The new file system item to insert.</param>
        /// <param name="collection">The already sorted collection.</param>
        /// <returns>The index at which to insert the new item.</returns>
        int GetInsertIndex(T newItem, ICollection<T> collection);

        /// <summary>
        /// Sorts the entire collection according to the current sorting rules.
        /// </summary>
        /// <param name="source">The items source.</param>
        /// <param name="destination">The destination collection to sort.</param>
        void SortCollection(IEnumerable<T> source, ICollection<T> destination);
    }
}
