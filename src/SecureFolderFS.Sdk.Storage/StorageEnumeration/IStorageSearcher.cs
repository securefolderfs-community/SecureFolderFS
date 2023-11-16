using SecureFolderFS.Sdk.Storage.NestedStorage;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Sdk.Storage.StorageEnumeration
{
    public interface IStorageSearcher
    {
        /// <summary>
        /// Gets the folder that is being searched in.
        /// </summary>
        IFolder SourceFolder { get; }

        /// <summary>
        /// Finds files in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for files.</param>
        /// <param name="predicate">The predicate to apply for every file found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="INestedFile"/> that contains found files.</returns>
        IAsyncEnumerable<INestedFile> FindFilesAsync(string searchPattern, Func<IFile, bool>? predicate = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds folders in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for folders.</param>
        /// <param name="predicate">The predicate to apply for every folder found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="INestedFolder"/> that contains found folders.</returns>
        IAsyncEnumerable<INestedFolder> FindFoldersAsync(string searchPattern, Func<IFolder, bool>? predicate = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds items in the <see cref="SourceFolder"/>.
        /// </summary>
        /// <param name="searchPattern">The pattern to use when searching for items.</param>
        /// <param name="predicate">The predicate to apply for every item found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="INestedStorable"/> that contains found items.</returns>
        IAsyncEnumerable<INestedStorable> FindStorageAsync(string searchPattern, Func<IStorable, bool>? predicate = default, CancellationToken cancellationToken = default);
    }
}
