using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultFileSystemService
    {
        /// <summary>
        /// Gets the local representation of a file system.
        /// </summary>
        /// <returns>A new local file system instance.</returns>
        Task<IFileSystem> GetLocalFileSystemAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets all file systems that are supported by the app.
        /// </summary>
        /// <remarks>
        /// Returned file systems that are available, may not be supported on this device. 
        /// Use <see cref="IFileSystem.GetStatusAsync"/> to check if a given file system is supported.
        /// </remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="IFileSystem"/> of available file systems.</returns>
        IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken);

        FileSystemOptions GetFileSystemOptions(IVaultModel vaultModel, string fileSystemId);
    }
}
