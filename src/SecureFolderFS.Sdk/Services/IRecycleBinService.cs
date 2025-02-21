using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.Services
{
    public interface IRecycleBinService
    {
        /// <summary>
        /// Toggles the recycle bin feature on or off.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="vfsRoot">The root of the virtual file system.</param>
        /// <param name="value">The value to set for the recycle bin state.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns a <see cref="Task"/> that represents the asynchronous operation. Value is a boolean indicating success or failure.</returns>
        Task<bool> ToggleRecycleBinAsync(IFolder vaultFolder, IVFSRoot vfsRoot, bool value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the items found in the recycle bin.
        /// </summary>
        /// <param name="vfsRoot">The root of the virtual file system.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="RecycleBinItemViewModel"/> of recycle bin items.</returns>
        IAsyncEnumerable<RecycleBinItemModel> GetRecycleBinItemsAsync(IVFSRoot vfsRoot, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores an item from the recycle bin.
        /// </summary>
        /// <param name="vfsRoot">The root of the virtual file system.</param>
        /// <param name="recycleBinItem">The item to restore.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of the operation.</returns>
        Task<IResult> RestoreItemAsync(IVFSRoot vfsRoot, IStorableChild recycleBinItem, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Permanently deletes an item from the recycle bin.
        /// </summary>
        /// <param name="recycleBinItem">The item to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of the operation.</returns>
        Task<IResult> DeletePermanentlyAsync(IStorableChild recycleBinItem, CancellationToken cancellationToken = default);
    }
}
