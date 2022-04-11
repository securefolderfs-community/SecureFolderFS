using System.Collections.Generic;

namespace SecureFolderFS.Shared.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddWithOverflow<T>(this IList<T> list, T item, int maxCapacity)
        {
            if (list.Count >= maxCapacity)
            {
                list.RemoveAt(0);
            }

            list.Add(item);
        }

        public static void EnumeratedAdd<T>(this ICollection<T> collection, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                collection.Add(item);
            }
        }
    }
}
