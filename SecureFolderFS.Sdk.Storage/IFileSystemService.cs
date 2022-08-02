using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Provides an abstract layer for accessing the file system.
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>
        /// Checks and requests permission to access file system.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is granted returns true, otherwise false.</returns>
        Task<bool> IsFileSystemAccessible();

        /// <summary>
        /// Check if file exists at specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file exists, returns true otherwise false.</returns>
        Task<bool> FileExistsAsync(string path);

        /// <summary>
        /// Check if directory exists at specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the directory exists, returns true otherwise false.</returns>
        Task<bool> DirectoryExistsAsync(string path);

        /// <summary>
        /// Gets the folder at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If folder is found and access is granted, returns <see cref="ILocatableFolder"/> otherwise null.</returns>
        Task<ILocatableFolder?> GetFolderFromPathAsync(string path);

        /// <summary>
        /// Gets the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file is found and access is granted, returns <see cref="ILocatableFile"/> otherwise null.</returns>
        Task<ILocatableFile?> GetFileFromPathAsync(string path);

        /// <summary>
        /// Locks the provided storage object and prevents the deletion of it.
        /// </summary>
        /// <param name="storage">The storage object to lock.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns a lock handle to <paramref name="storage"/> represented with <see cref="IDisposable"/>, otherwise null.</returns>
        Task<IDisposable?> ObtainLockAsync(IStorable storage);
    }
}
