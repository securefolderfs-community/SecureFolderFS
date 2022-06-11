using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StorageEnumeration
{
    /// <summary>
    /// Enumerates storage objects of given directory.
    /// <remarks>
    /// This interface can be implemented to provide complex enumeration of directories as well as being a substitute for built-in <see cref="IFolder"/> enumeration.</remarks>
    /// </summary>
    public interface IStorageEnumerator : IDisposable
    {
        /// <summary>
        /// Gets the folder where enumeration takes place.
        /// </summary>
        IFolder SourceFolder { get; }

        /// <summary>
        /// Enumerates the <see cref="SourceFolder"/> for files.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IFile"/> of all files discovered by the enumerator.</returns>
        Task<IEnumerable<IFile>> EnumerateFilesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerates the <see cref="SourceFolder"/> for folders.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IFolder"/> of all folders discovered by the enumerator.</returns>
        Task<IEnumerable<IFolder>> EnumerateFoldersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerates the <see cref="SourceFolder"/> for items.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IEnumerable{T}"/> of type <see cref="IBaseStorage"/> of all items discovered by the enumerator.</returns>
        Task<IEnumerable<IBaseStorage>> EnumerateStorageAsync(CancellationToken cancellationToken = default);
    }
}
