using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    public interface IFileSystem
    {
        /// <summary>
        /// Gets an unique id associated with this file system.
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
        Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default);
        
        Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, CancellationToken cancellationToken = default);
    }
}
