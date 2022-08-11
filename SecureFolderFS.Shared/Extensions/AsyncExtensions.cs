using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AsyncExtensions
    {
        public static async Task<IReadOnlyList<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();
            await foreach (var item in asyncEnumerable)
            {
                list.Add(item);
            }

            return list;
        }
    }
}
