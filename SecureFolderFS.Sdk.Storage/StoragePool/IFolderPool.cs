using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StoragePool
{
    /// <summary>
    /// Represents a folder store to retrieve new folders from a common pool.
    /// </summary>
    public interface IFolderPool
    {
        /// <summary>
        /// Clears the pool of any folders.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the pool has been cleared, returns true otherwise false.</returns>
        Task<bool> ClearPoolAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a new or existing folder from the pool.
        /// </summary>
        /// <param name="fileName">The name of the folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <see cref="IFolder"/> from the pool otherwise null.</returns>
        Task<IFolder?> RequestFolderAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
