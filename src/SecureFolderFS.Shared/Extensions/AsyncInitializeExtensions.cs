using System;
using System.Threading;
using System.Threading.Tasks;
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

        /// <summary>
        /// Tries to asynchronously perform initialization.
        /// </summary>
        /// <param name="asyncInitialize">The <see cref="IAsyncInitialize"/> instance to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        public static async Task<bool> TryInitAsync(this IAsyncInitialize asyncInitialize, CancellationToken cancellationToken = default)
        {
            try
            {
                await asyncInitialize.InitAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
