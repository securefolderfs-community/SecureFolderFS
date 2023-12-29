using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to start and manage app updates.
    /// </summary>
    public interface IUpdateService : INotifyStateChanged
    {
        /// <summary>
        /// Checks whether the app supports updates.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The value is true if updates are supported; otherwise false.</returns>
        Task<bool> IsSupportedAsync();

        /// <summary>
        /// Checks whether there are available updates.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If available updates were found, returns true otherwise false.</returns>
        Task<bool> IsUpdateAvailableAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts all app updates in the update queue.
        /// </summary>
        /// <param name="progress">The progress of the operation. The reported values range from 0 to 100 inclusive.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of the operation. .</returns>
        Task<IResult> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default);
    }
}
