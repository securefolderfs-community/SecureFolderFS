using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Provides an abstract layer for accessing the file system.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Check if file exists at specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file exists, returns true otherwise false.</returns>
        Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if directory exists at specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the directory exists, returns true otherwise false.</returns>
        Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the folder at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If folder is found and access is granted, returns <see cref="ILocatableFolder"/> otherwise throws an exception.</returns>
        Task<ILocatableFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file is found and access is granted, returns <see cref="ILocatableFile"/> otherwise throws an exception.</returns>
        Task<ILocatableFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default);
    }
}
