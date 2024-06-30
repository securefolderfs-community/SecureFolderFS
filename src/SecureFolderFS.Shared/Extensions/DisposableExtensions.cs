using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class DisposableExtensions
    {
        public static async Task TryDisposeAsync(object obj)
        {
            switch (obj)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;

                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        public static void TryDispose(object obj)
        {
            switch (obj)
            {
                case IDisposable disposable:
                    disposable.Dispose();
                    break;

                case IAsyncDisposable asyncDisposable:
                    _ = asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
            }
        }
    }
}
