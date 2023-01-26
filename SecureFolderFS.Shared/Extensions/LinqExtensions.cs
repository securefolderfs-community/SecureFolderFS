using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Shared.Extensions
{
    public static class LinqExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T>? enumerable)
            => enumerable is null || !enumerable.Any();

        public static void DisposeCollection<T>(this IEnumerable<T?> enumerable)
            where T : IDisposable
        {
            foreach (var item in enumerable)
            {
                item?.Dispose();
            }
        }

        public static IEnumerable<T> IfEmptyThenAppend<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable.IsEmpty())
                return enumerable.Append(item);

            return enumerable;
        }
    }
}
