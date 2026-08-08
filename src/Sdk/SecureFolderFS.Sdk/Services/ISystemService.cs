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

        /// <summary>
        /// Determines whether the app is registered to start automatically on system startup.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true if auto start is enabled; otherwise false.</returns>
        Task<bool> IsAutoStartEnabledAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers or unregisters the app to start automatically on system startup.
        /// </summary>
        /// <param name="isEnabled">Determines whether to register or unregister the app for auto start.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true if the registration was updated successfully; otherwise false.</returns>
        Task<bool> TrySetAutoStartAsync(bool isEnabled, CancellationToken cancellationToken = default);
     }
}
