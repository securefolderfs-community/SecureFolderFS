using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Provides functionality to manage and interact with a recycle bin for a vault.
    /// </summary>
    public interface IRecycleBinService
    {
        /// <summary>
        /// Configures the recycle bin for a specific unlocked vault.
        /// </summary>
        /// <param name="vfsRoot">The root of the virtual file system.</param>
        /// <param name="maxSize">The maximum size of the recycle bin in bytes.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ConfigureRecycleBinAsync(IVFSRoot vfsRoot, long maxSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the recycle bin folder for a specific unlocked vault.
        /// </summary>
        /// <param name="vfsRoot">The root of the virtual file system.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns an <see cref="IRecycleBinFolder"/> instance.</returns>
        Task<IRecycleBinFolder> GetRecycleBinAsync(IVFSRoot vfsRoot, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or creates the recycle bin folder for a specific unlocked vault.
        /// </summary>
        /// <param name="vfsRoot">The root of the virtual file system.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns an <see cref="IRecycleBinFolder"/> instance.</returns>
        Task<IRecycleBinFolder> GetOrCreateRecycleBinAsync(IVFSRoot vfsRoot, CancellationToken cancellationToken = default);
    }
}
