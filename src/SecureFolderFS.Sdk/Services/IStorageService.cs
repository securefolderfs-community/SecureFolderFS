using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

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
        /// <typeparam name="TStorable">The requested type of storable to get the bookmark for.</typeparam>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the item associated with the bookmark ID is found, returns <see cref="IStorable"/>.</returns>
        Task<TStorable> GetPersistedAsync<TStorable>(string id, CancellationToken cancellationToken = default) where TStorable : IStorable;

        /// <summary>
        /// Removes application access from bookmarked file system resource.
        /// </summary>
        /// <param name="storable">The <see cref="IStorable"/> instance that was bookmarked to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RemovePersistedAsync(IStorable storable, CancellationToken cancellationToken = default);
    }
}
