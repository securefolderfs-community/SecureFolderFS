using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Provides metadata for a file system provider.
    /// </summary>
    public interface IFileSystemInfo
    {
        /// <summary>
        /// Gets the unique id associated with this file system.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of this file system.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines whether this file system is available on the device.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that represents the status of the file system provider.</returns>
        Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously mounts a virtual file system instance rooted at the specified vault folder.
        /// </summary>
        /// <param name="vaultFolder">The folder representing the root of the vault to mount.</param>
        /// <param name="unlockContract">A disposable contract that provides credentials of the vault.</param>
        /// <param name="options">A dictionary of additional options for mounting the vault. The specific keys and values depend on the implementation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the mount operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an <see cref="IVFSRoot"/> instance representing the mounted virtual file system.</returns>
        Task<IVFSRoot> MountAsync(IFolder vaultFolder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default);
    }
}
