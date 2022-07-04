using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.Enums;

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
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If folder is found and access is granted, returns <see cref="IFolder"/> otherwise null.</returns>
        Task<IFolder?> GetFolderFromPathAsync(string path);

        /// <summary>
        /// Gets the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file is found and access is granted, returns <see cref="IFile"/> otherwise null.</returns>
        Task<IFile?> GetFileFromPathAsync(string path);

        /// <summary>
        /// Locks the provided storage object and prevents the deletion of it.
        /// </summary>
        /// <param name="storage">The storage object to lock.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns a <see cref="IDisposable"/> lock handle to <paramref name="storage"/>, otherwise false.</returns>
        Task<IDisposable?> ObtainLockAsync(IBaseStorage storage);

        /// <summary>
        /// Copies the storage object to the <paramref name="destinationFolder"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of storage to copy.</typeparam>
        /// <param name="source">The storage object to copy.</param>
        /// <param name="destinationFolder">The destination folder to copy the storage object to.</param>
        /// <param name="options">Determines how to handle the collision in case an object with the same name already exists.</param>
        /// <param name="progress">Reports the progress of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <typeparamref name="TSource"/> of the copied object, otherwise null.</returns>
        Task<TSource?> CopyAsync<TSource>(TSource source, IFolder destinationFolder, NameCollisionOption options, IProgress<double>? progress = null, CancellationToken cancellationToken = default) where TSource : IBaseStorage;

        /// <summary>
        /// Moves the storage object to the <paramref name="destinationFolder"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of storage to move.</typeparam>
        /// <param name="source">The storage object to move.</param>
        /// <param name="destinationFolder">The destination folder to move the storage object to.</param>
        /// <param name="options">Determines how to handle the collision in case an object with the same name already exists.</param>
        /// <param name="progress">Reports the progress of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <typeparamref name="TSource"/> of the moved object, otherwise null.</returns>
        Task<TSource?> MoveAsync<TSource>(TSource source, IFolder destinationFolder, NameCollisionOption options, IProgress<double>? progress = null, CancellationToken cancellationToken = default) where TSource : IBaseStorage;
    }
}
