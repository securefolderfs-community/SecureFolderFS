using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Provides an abstract storage layer for accessing the file system.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets the application folder.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IFolder> GetAppFolderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the item with associated bookmark <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique bookmark ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the item associated with the bookmark ID is found, returns <see cref="IStorable"/>.</returns>
        Task<IStorable> GetBookmarkAsync(string id, CancellationToken cancellationToken = default);
    }
}
