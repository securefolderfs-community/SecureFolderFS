using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <inheritdoc cref="RenameStorableAsync{TStorable}(IModifiableFolder,TStorable,string,IProgress{OwlCore.Storage.IStorable},CancellationToken)"/>
        public static Task<TStorable> RenameStorableAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToRename, string newName, CancellationToken cancellationToken = default)
            where TStorable : IStorableChild
        {
            return RenameStorableAsync(destinationFolder, itemToRename, newName, null, cancellationToken);
        }

        /// <summary>
        /// Renames a storable in the destination folder.
        /// </summary>
        /// <typeparam name="TStorable">The type of storable, whether a <see cref="IFile"/> or a <see cref="IFolder"/>.</typeparam>
        /// <param name="destinationFolder">The folder where the storable is moved to.</param>
        /// <param name="itemToRename">The storable being renamed.</param>
        /// <param name="newName">The new name of the renamed item.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the renamed storable item.</returns>
        public static async Task<TStorable> RenameStorableAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToRename, string newName, IProgress<IStorable>? reporter, CancellationToken cancellationToken = default)
            where TStorable : IStorableChild
        {
            return itemToRename switch
            {
                IChildFile fileToRename => (TStorable)await destinationFolder.RenameAsync(fileToRename, newName, cancellationToken),
                IModifiableFolder folderToRename => (TStorable)await destinationFolder.RenameAsync(folderToRename, newName, reporter, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(itemToRename))
            };
        }

        /// <summary>
        /// Renames a given file in the destination folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the file is renamed in.</param>
        /// <param name="fileToRename">The file being renamed.</param>
        /// <param name="newName">The new name of the renamed file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the renamed file.</returns>
        public static async Task<IChildFile> RenameAsync(this IModifiableFolder destinationFolder, IChildFile fileToRename, string newName, CancellationToken cancellationToken = default)
        {
            if (destinationFolder is IRenamableFolder renamableFolder)
                return (IChildFile)await renamableFolder.RenameAsync(fileToRename, newName, cancellationToken);

            return await destinationFolder.MoveFromAsync(fileToRename, destinationFolder, false, newName, cancellationToken);
        }

        /// <summary>
        /// Renames a given folder in the destination folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the folder is renamed in.</param>
        /// <param name="folderToRename">The folder being renamed.</param>
        /// <param name="newName">The new name of the renamed folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the renamed folder.</returns>
        public static async Task<IChildFolder> RenameAsync(this IModifiableFolder destinationFolder, IModifiableFolder folderToRename, string newName, CancellationToken cancellationToken = default)
        {
            return await RenameAsync(destinationFolder, folderToRename, newName, null, cancellationToken);
        }

        /// <summary>
        /// Renames a given folder in the destination folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the folder is renamed in.</param>
        /// <param name="folderToRename">The folder being renamed.</param>
        /// <param name="newName">The new name of the renamed folder.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the renamed folder.</returns>
        public static async Task<IChildFolder> RenameAsync(this IModifiableFolder destinationFolder, IModifiableFolder folderToRename, string newName, IProgress<IStorable>? reporter, CancellationToken cancellationToken = default)
        {
            if (destinationFolder is IRenamableFolder renamableFolder)
                return (IChildFolder)await renamableFolder.RenameAsync((IStorableChild)folderToRename, newName, cancellationToken);

            return await destinationFolder.MoveFromAsync(folderToRename, destinationFolder, false, newName, reporter, cancellationToken);
        }
    }
}
