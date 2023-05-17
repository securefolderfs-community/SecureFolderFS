using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to start and manage app updates.
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Checks whether the app supports updates.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The value is true if updates are supported, otherwise false.</returns>
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
        /// <param name="progress">The progress of the operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result of the operation is determined by <see cref="AppUpdateResultType"/>.</returns>
        Task<AppUpdateResultType> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default);
    }
}
