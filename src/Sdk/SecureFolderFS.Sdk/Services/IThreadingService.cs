using System.Threading;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to manage current thread execution.
    /// </summary>
    public interface IThreadingService
    {
        /// <summary>
        /// Gets the <see cref="SynchronizationContext"/> of the UI thread.
        /// </summary>
        /// <returns>The UI thread context, if available.</returns>
        SynchronizationContext? GetContext();

        /// <summary>
        /// Changes current thread to execute on UI thread.
        /// </summary>
        /// <returns>An <see cref="IAwaitable"/> that represents the asynchronous operation.</returns>
        IAwaitable ChangeThreadAsync();
    }
}
