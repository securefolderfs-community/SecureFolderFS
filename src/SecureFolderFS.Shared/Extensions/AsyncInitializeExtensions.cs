using System.Threading;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AsyncInitializeExtensions
    {
        public static T WithInitAsync<T>(this T asyncInitialize, CancellationToken cancellationToken = default)
            where T : IAsyncInitialize
        {
            _ = asyncInitialize.InitAsync(cancellationToken);
            return asyncInitialize;
        }
    }
}
