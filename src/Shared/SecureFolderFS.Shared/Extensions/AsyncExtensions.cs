using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AsyncExtensions
    {
        public static async ValueTask<T[]> ToArrayAsyncImpl<T>(this IAsyncEnumerable<T> source,
            CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                list.Add(item);
            }

            return list.ToArray();
        }

        public static async ValueTask<T?> FirstOrDefaultAsyncImpl<T>(this IAsyncEnumerable<T> source,
            Func<T, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                if (predicate(item))
                    return item;
            }

            return default;
        }

        public static async IAsyncEnumerable<TResult> SelectAsyncImpl<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                yield return selector(item);
            }
        }
    }
}
