using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T>? enumerable)
        {
            if (enumerable is null)
                return true;

            if (enumerable is IList { Count: 0 })
                return true;

            return !enumerable.Any();
        }

        public static void DisposeElements<T>(this IEnumerable<T?> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }
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

        public static void AddWithMaxCapacity<T>(this IList<T> list, T item, int maxCapacity)
        {
            if (list.Count >= maxCapacity)
                list.RemoveAt(0);

            list.Add(item);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }
}
