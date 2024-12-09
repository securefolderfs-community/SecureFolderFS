using OwlCore.Storage;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Storage.Scanners
{
    /// <summary>
    /// Handles discovery of items represented by <typeparamref name="T"/> in a given folder.
    /// </summary>
    public interface IFolderScanner<out T> where T : IStorableChild
    {
        /// <summary>
        /// The root folder to scan for items.
        /// </summary>
        IFolder RootFolder { get; }

        /// <summary>
        /// Scans a folder and all subfolders for items.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <typeparamref name="T"/> of found items.</returns>
        IAsyncEnumerable<T> ScanFolderAsync(CancellationToken cancellationToken = default);
    }
}
