using System;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to manage current thread execution.
    /// </summary>
    [Obsolete("This service has been deprecated. Use SynchronizationContext instead for thread manipulation.")]
    public interface IThreadingService
    {
        /// <summary>
        /// Changes current thread to execute on UI thread.
        /// </summary>
        /// <returns>A <see cref="IAwaitable"/> that represents the asynchronous operation.</returns>
        IAwaitable ExecuteOnUiThreadAsync();

        /// <summary>
        /// Executes specified <paramref name="action"/> on UI thread.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ExecuteOnUiThreadAsync(Action action);
    }
}
