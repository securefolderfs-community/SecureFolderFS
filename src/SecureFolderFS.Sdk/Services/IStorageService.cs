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
        /// Gets the item with associated <paramref name="persistableId"/>.
        /// </summary>
        /// <param name="persistableId">A unique Persistance ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <typeparam name="TStorable">The requested type of storable to get the bookmark for.</typeparam>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the item associated with the Persistence ID is found, returns <see cref="IStorable"/>.</returns>
        Task<TStorable> GetPersistedAsync<TStorable>(string persistableId, CancellationToken cancellationToken = default) where TStorable : IStorable;
    }
}
