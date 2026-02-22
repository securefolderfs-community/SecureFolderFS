using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class DisposableExtensions
    {
        public static async Task TryDisposeAsync(object presumedDisposable)
        {
            switch (presumedDisposable)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;

                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        public static void TryDispose(object presumedDisposable)
        {
            switch (presumedDisposable)
            {
                case IDisposable disposable:
                    disposable.Dispose();
                    break;

                case IAsyncDisposable asyncDisposable:
                    asyncDisposable.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                    break;
            }
        }
    }
}
