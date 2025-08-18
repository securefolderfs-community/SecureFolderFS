using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to control system related actions and events.
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        /// Occurs when the user locks their device.
        /// </summary>
        event EventHandler? DeviceLocked;

        /// <summary>
        /// Gets the available remaining free space in bytes on the users' device.
        /// </summary>
        /// <param name="storageRoot">The root folder of the given storage cluster to get the size from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the amount of usable storage space in bytes.</returns>
        Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default);
     }
}
