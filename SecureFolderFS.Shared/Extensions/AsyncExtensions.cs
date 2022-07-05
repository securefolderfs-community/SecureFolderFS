using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AsyncExtensions
    {
        public static IEnumerable<Task> CombineAsyncInitialize(CancellationToken cancellationToken, params IAsyncInitialize[] items)
        {
            foreach (var item in items)
            {
                yield return item.InitAsync(cancellationToken);
            }
        }
    }
}
