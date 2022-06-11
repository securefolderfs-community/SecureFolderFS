using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StorageEnumeration
{
    public interface IStorageSearcher
    {
        /// <summary>
        /// Gets the folder that's being searched in.
        /// </summary>
        IFolder SourceFolder { get; }

        /// <summary>
        /// Finds files in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for files.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IFile"/> that contains found files.</returns>
        Task<IEnumerable<IFile>> FindFilesAsync(string searchPattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds files in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for files.</param>
        /// <param name="predicate">The predicate to apply for every file found.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IFile"/> that contains found files.</returns>
        Task<IEnumerable<IFile>> FindFilesAsync(string searchPattern, Func<IFile, bool>? predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds folders in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for folders.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IFolder"/> that contains found folders.</returns>
        Task<IEnumerable<IFolder>> FindFoldersAsync(string searchPattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds folders in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for folders.</param>
        /// <param name="predicate">The predicate to apply for every folder found.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IFolder"/> that contains found folders.</returns>
        Task<IEnumerable<IFolder>> FindFoldersAsync(string searchPattern, Func<IFolder, bool>? predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds items in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for items.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IBaseStorage"/> that contains found items.</returns>
        Task<IEnumerable<IBaseStorage>> FindStorageAsync(string searchPattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds items in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for items.</param>
        /// <param name="predicate">The predicate to apply for every item found.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IBaseStorage"/> that contains found items.</returns>
        Task<IEnumerable<IBaseStorage>> FindStorageAsync(string searchPattern, Func<IBaseStorage, bool>? predicate, CancellationToken cancellationToken = default);
    }
}
