using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AsyncExtensions
    {
        public static async Task<IReadOnlyList<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            await foreach (var item in asyncEnumerable.WithCancellation(cancellationToken))
            {
                list.Add(item);
            }

            return list;
        }

        public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken cancellationToken = default)
        {
            await using var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
            return asyncEnumerator.Current ?? default;
        }
    }
}
