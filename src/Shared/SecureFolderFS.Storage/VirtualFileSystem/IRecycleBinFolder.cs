using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Represents a recycle bin folder.
    /// </summary>
    public interface IRecycleBinFolder : IModifiableFolder, ISizeOf
    {
        /// <summary>
        /// Restores a collection of items from the recycle bin.
        /// </summary>
        /// <remarks>
        /// Every item is restored to its original location when it still exists. The <paramref name="folderPicker"/>
        /// is invoked at most once for the items whose original location could not be used.
        /// <br/><br/>
        /// The <paramref name="items"/> may only accept the ciphertext implementation or <see cref="IRecycleBinItem"/>.
        /// </remarks>
        /// <param name="items">The items to restore.</param>
        /// <param name="folderPicker">The <see cref="IFolderPicker"/> instance to use when the destination folder no longer exists.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RestoreItemsAsync(IEnumerable<IStorableChild> items, IFolderPicker folderPicker, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single recycle bin item that matches the given ciphertext (on-disk) name, if one exists.
        /// </summary>
        /// <param name="ciphertextName">The on-disk name of the payload inside the recycle bin.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the matched item, or null.</returns>
        Task<IStorableChild?> TryGetItemAsync(string ciphertextName, CancellationToken cancellationToken = default);
    }
}
