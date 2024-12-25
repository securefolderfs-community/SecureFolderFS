using OwlCore.Storage;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Storage.Scanners
{
    /// <summary>
    /// Handles discovery of items in a given folder.
    /// </summary>
    public interface IFolderScanner
    {
        /// <summary>
        /// The root folder to scan for items.
        /// </summary>
        IFolder RootFolder { get; }

        /// <summary>
        /// Scans a folder and all subfolders for items.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IStorableChild"/> of found items.</returns>
        IAsyncEnumerable<IStorableChild> ScanFolderAsync(CancellationToken cancellationToken = default);
    }
}
