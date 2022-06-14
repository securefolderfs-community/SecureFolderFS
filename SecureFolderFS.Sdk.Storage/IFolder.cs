using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.Enums;

namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Represents a folder on the file system.
    /// </summary>
    public interface IFolder : IBaseStorage
    {
        /// <summary>
        /// Creates a new file with specified <paramref name="desiredName"/> in the current folder.
        /// </summary>
        /// <param name="desiredName">The name to create the file with.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns a <see cref="IFile"/> to the created object.</returns>
        Task<IFile> CreateFileAsync(string desiredName);

        /// <summary>
        /// Creates a new file with specified <paramref name="desiredName"/> in the current folder.
        /// </summary>
        /// <param name="desiredName">The name to create the file with.</param>
        /// <param name="options">Determines how to handle the collision in case a file already exists with the <paramref name="desiredName"/> name.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns a <see cref="IFile"/> to the created object.</returns>
        Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption options);

        /// <summary>
        /// Creates a new folder with specified <paramref name="desiredName"/> in the current folder.
        /// </summary>
        /// <param name="desiredName">The name to create the folder with.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns a <see cref="IFolder"/> to the created object.</returns>
        Task<IFolder> CreateFolderAsync(string desiredName);

        /// <summary>
        /// Creates a new folder with specified <paramref name="desiredName"/> in the current folder.
        /// </summary>
        /// <param name="desiredName">The name to create the folder with.</param>
        /// <param name="options">Determines how to handle the collision in case a folder already exists with the <paramref name="desiredName"/> name.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns a <see cref="IFile"/> to the created object.</returns>
        Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption options);

        /// <summary>
        /// Gets a file in the current directory by name.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If file is found and access is granted, returns <see cref="IFile"/>, otherwise null.</returns>
        Task<IFile?> GetFileAsync(string fileName);

        /// <summary>
        /// Gets a folder in the current directory by name.
        /// </summary>
        /// <param name="folderName">The name of the folder.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If folder is found and access is granted, returns <see cref="IFolder"/>, otherwise null.</returns>
        Task<IFolder?> GetFolderAsync(string folderName);

        /// <summary>
        /// Gets all files in the current directory.
        /// </summary>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFile"/> of files in the directory.</returns>
        IAsyncEnumerable<IFile> GetFilesAsync();

        /// <summary>
        /// Gets all folders in the current directory.
        /// </summary>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFolder"/> of folders in the directory.</returns>
        IAsyncEnumerable<IFolder> GetFoldersAsync();

        /// <summary>
        /// Gets all items in the current directory.
        /// </summary>
        /// <returns>Returns an async operation represented by <see cref="IEnumerable{T}"/> of type <see cref="IBaseStorage"/> of items in the directory.</returns>
        IAsyncEnumerable<IBaseStorage> GetStorageAsync();
    }
}
