using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <inheritdoc cref="MoveStorableFromAsync{TStorable}(IModifiableFolder,TStorable,IModifiableFolder,bool,System.IProgress{OwlCore.Storage.IStorable},CancellationToken)"/>
        public static Task<TStorable> MoveStorableFromAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
            where TStorable : IStorableChild
        {
            return MoveStorableFromAsync(destinationFolder, itemToMove, source, overwrite, null, cancellationToken);
        }

        /// <summary>
        /// Moves a storable from the source folder into the destination folder. Returns the new storable that resides in the destination folder.
        /// </summary>
        /// <typeparam name="TStorable">The type of storable, whether a <see cref="IFile"/> or a <see cref="IFolder"/>.</typeparam>
        /// <param name="destinationFolder">The folder where the storable is moved to.</param>
        /// <param name="itemToMove">The storable being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="itemToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<TStorable> MoveStorableFromAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToMove, IModifiableFolder source, bool overwrite, IProgress<IStorable>? reporter, CancellationToken cancellationToken = default)
            where TStorable : IStorableChild
        {
            return itemToMove switch
            {
                IChildFile fileToMove => (TStorable)await destinationFolder.MoveFromAsync(fileToMove, source, overwrite, cancellationToken),
                IModifiableFolder folderToMove => (TStorable)await destinationFolder.MoveFromAsync(folderToMove, source, overwrite, folderToMove.Name, reporter, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(itemToMove))
            };
        }

        /// <inheritdoc cref="MoveFromAsync(IModifiableFolder,IModifiableFolder,IModifiableFolder,bool,string,IProgress{IStorable},CancellationToken)"/>
        public static Task<IChildFolder> MoveFromAsync(this IModifiableFolder destinationFolder, IModifiableFolder folderToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
        {
            return MoveFromAsync(destinationFolder, folderToMove, source, overwrite, folderToMove.Name, null, cancellationToken);
        }

        /// <inheritdoc cref="MoveFromAsync(IModifiableFolder,IModifiableFolder,IModifiableFolder,bool,string,IProgress{IStorable},CancellationToken)"/>
        public static Task<IChildFolder> MoveFromAsync(this IModifiableFolder destinationFolder, IModifiableFolder folderToMove, IModifiableFolder source, bool overwrite, string newName, CancellationToken cancellationToken = default)
        {
            return MoveFromAsync(destinationFolder, folderToMove, source, overwrite, newName, null, cancellationToken);
        }

        /// <summary>
        /// Moves a folder from the source folder into the destination folder. Returns the new folder that resides in the destination folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the folder is moved to.</param>
        /// <param name="folderToMove">The folder being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="folderToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="newName">The new name of the created folder.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<IChildFolder> MoveFromAsync(this IModifiableFolder destinationFolder,
            IModifiableFolder folderToMove, IModifiableFolder source, bool overwrite, string newName,
            IProgress<IStorable>? reporter = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create the corresponding folder in the destination
            var movedFolder = await destinationFolder.CreateFolderAsync(newName, overwrite, cancellationToken);
            if (movedFolder is not IModifiableFolder modifiableMovedFolder)
                throw new UnauthorizedAccessException("The created folder is not modifiable.");

            reporter?.Report(movedFolder);

            var stack = new Stack<(IModifiableFolder FolderToMove, IModifiableFolder Destination)>();
            stack.Push((folderToMove, modifiableMovedFolder));

            while (stack.Count > 0)
            {
                var (currentFolder, currentDestination) = stack.Pop();
                await foreach (var item in currentFolder.GetItemsAsync(StorableType.All, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    switch (item)
                    {
                        case IChildFile file:
                        {
                            var movedFile = await currentDestination.MoveFromAsync(file, currentFolder, overwrite, cancellationToken);
                            reporter?.Report(movedFile);
                            break;
                        }

                        case IModifiableFolder subFolder:
                        {
                            var createdSubFolder = await currentDestination.CreateFolderAsync(subFolder.Name, overwrite, cancellationToken);
                            if (createdSubFolder is not IModifiableFolder modifiableCreatedSubFolder)
                                throw new UnauthorizedAccessException("The created folder is not modifiable.");

                            reporter?.Report(createdSubFolder);
                            stack.Push((subFolder, modifiableCreatedSubFolder));
                            break;
                        }
                    }
                }
            }

            if (folderToMove is IChildFolder childFolderToMove)
                await source.DeleteAsync(childFolderToMove, deleteImmediately: true, cancellationToken: cancellationToken);

            return movedFolder;
        }
    }
}
