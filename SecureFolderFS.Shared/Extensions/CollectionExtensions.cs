using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T>? enumerable)
            => enumerable is null || !enumerable.Any();

        public static void DisposeCollection<T>(this IEnumerable<T?> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        public static bool TryFirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, [NotNullWhen(true)] out TSource? item)
            where TSource : notnull
        {
            using var iterator = source.GetEnumerator();
            if (!iterator.MoveNext() || !predicate(iterator.Current))
            {
                item = default;
                return false;
            }

            item = iterator.Current;
            return true;
        }

        public static async Task<IReadOnlyList<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            await foreach (var item in asyncEnumerable.WithCancellation(cancellationToken))
            {
                list.Add(item);
            }

            return list;
        }

        public static int FindIndex<T>(this IList<T> source, Predicate<T> match)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (match(source[i]))
                    return i;
            }

            return -1;
        }

        public static void AddWithMaxCapacity<T>(this IList<T> list, T item, int maxCapacity)
        {
            if (list.Count >= maxCapacity)
                list.RemoveAt(0);

            list.Add(item);
        }
    }
}
