using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is granted returns true, otherwise false.</returns>
        Task<bool> IsFileSystemAccessibleAsync(CancellationToken cancellationToken = default);

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
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If folder is found and access is granted, returns <see cref="ILocatableFolder"/> otherwise null.</returns>
        Task<ILocatableFolder?> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file is found and access is granted, returns <see cref="ILocatableFile"/> otherwise null.</returns>
        Task<ILocatableFile?> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Locks the provided storage object and prevents the deletion of it.
        /// </summary>
        /// <param name="storage">The storage object to lock.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns a lock handle to <paramref name="storage"/> represented with <see cref="IDisposable"/>, otherwise null.</returns>
        Task<IDisposable?> ObtainLockAsync(IStorable storage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all free locations of free mount points.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of mount point paths.</returns>
        IEnumerable<string> GetFreeMountPoints();
    }
}
