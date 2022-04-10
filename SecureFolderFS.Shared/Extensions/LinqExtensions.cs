using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Shared.Extensions
{
    public static class LinqExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T>? enumerable) => enumerable == null || !enumerable.Any();

        public static void DisposeCollection<T>(this IEnumerable<T> enumerable)
            where T : IDisposable
        {
            foreach (var item in enumerable)
            {
                item.Dispose();
            }
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
