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
    }
}
