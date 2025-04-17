using OwlCore.Storage;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.StorageProperties;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    public interface IRecycleBinFolder : IModifiableFolder, IStorableProperties
    {
        /// <summary>
        /// Restores a collection of items from the recycle bin.
        /// </summary>
        /// <remarks>
        /// If the collection contains more than one element, the <paramref name="folderPicker"/> is always invoked once, otherwise,
        /// the recycle bin tries to restore the item into its original location resorting to the <see cref="IFolderPicker"/> when necessary.
        /// <br/><br/>
        /// The <paramref name="items"/> may only accept the ciphertext implementation or <see cref="IRecycleBinItem"/>.
        /// </remarks>
        /// <param name="items">The items to restore.</param>
        /// <param name="folderPicker">The <see cref="IFolderPicker"/> instance to use when the destination folder no longer exists.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RestoreItemsAsync(IEnumerable<IStorableChild> items, IFolderPicker folderPicker, CancellationToken cancellationToken = default);
    }
}
