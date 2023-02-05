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

        public static bool TryFirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource? value)
        {
            using var iterator = source.GetEnumerator();
            if (!iterator.MoveNext() || !predicate(iterator.Current))
            {
                value = default;
                return false;
            }

            value = iterator.Current;
            return true;
        }
    }
}
