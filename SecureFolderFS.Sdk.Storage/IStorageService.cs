using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Provides an abstract storage layer for accessing the file system.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="id">The path to the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file is found and access is granted, returns <see cref="ILocatableFile"/> otherwise throws an exception.</returns>
        Task<IFile> GetFileAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the folder at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="id">The path to the folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If folder is found and access is granted, returns <see cref="ILocatableFolder"/> otherwise throws an exception.</returns>
        Task<IFolder> GetFolderAsync(string id, CancellationToken cancellationToken = default);
    }
}
