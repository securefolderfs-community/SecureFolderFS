using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StoragePool
{
    /// <summary>
    /// Represents a file store to retrieve new files from a common pool.
    /// </summary>
    public interface IFilePool
    {
        /// <summary>
        /// Clears the pool of any files.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the pool has been cleared returns true, otherwise false.</returns>
        Task<bool> ClearPoolAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a new or existing file from the pool.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns <see cref="IFile"/> from the pool, otherwise null.</returns>
        Task<IFile?> RequestFileAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
