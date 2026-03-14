using System;
using System.Collections.Generic;
using System.IO;
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
        /// Moves a storable from the source folder into the destination folder.
        /// </summary>
        /// <typeparam name="TStorable">The type of storable, whether a <see cref="IFile"/> or a <see cref="IFolder"/>.</typeparam>
        /// <param name="destinationFolder">The folder where the storable is moved to.</param>
        /// <param name="itemToMove">The storable being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="itemToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the moved storable item.</returns>
        public static async Task<TStorable> MoveStorableFromAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToMove, IModifiableFolder source, bool overwrite, IProgress<IStorable>? reporter, CancellationToken cancellationToken = default)
            where TStorable : IStorableChild
        {
            return await MoveStorableFromAsync(destinationFolder, itemToMove, source, overwrite, itemToMove.Name, reporter, cancellationToken);
        }

        /// <summary>
        /// Moves a storable from the source folder into the destination folder.
        /// </summary>
        /// <typeparam name="TStorable">The type of storable, whether a <see cref="IFile"/> or a <see cref="IFolder"/>.</typeparam>
        /// <param name="destinationFolder">The folder where the storable is moved to.</param>
        /// <param name="itemToMove">The storable being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="itemToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="newName">The new name of the moved item.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the moved storable item.</returns>
        public static async Task<TStorable> MoveStorableFromAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToMove, IModifiableFolder source, bool overwrite, string newName, IProgress<IStorable>? reporter, CancellationToken cancellationToken = default)
            where TStorable : IStorableChild
        {
            return itemToMove switch
            {
                IChildFile fileToMove => (TStorable)await destinationFolder.MoveFileImmediatelyFrom(fileToMove, source, overwrite, newName, cancellationToken),
                IModifiableFolder folderToMove => (TStorable)await destinationFolder.MoveFromAsync(folderToMove, source, overwrite, newName, reporter, cancellationToken),
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
        /// Moves a file out of the source folder and into the destination folder. Returns the new file that resides in this folder.
        /// </summary>
        /// <remarks>
        /// If the destination folder does not implement <see cref="IMoveRenamedFrom"/>, copy and delete operations take precedence.
        /// The delete operation, if supported, deletes the item immediately, instead of recycling it.
        /// </remarks>
        /// <param name="destinationFolder">The folder where the file is moved to.</param>
        /// <param name="fileToMove">The file being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="fileToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination file can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="newName">The name to use for the created file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <exception cref="FileAlreadyExistsException">Thrown when <paramref name="overwrite"/> is false and the resource being created already exists.</exception>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the moved file.</returns>
        public static async Task<IChildFile> MoveFileImmediatelyFrom(this IModifiableFolder destinationFolder, IChildFile fileToMove, IModifiableFolder source, bool overwrite, string newName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If the destination file exists and overwrite is false, it shouldn't be overwritten or returned as-is. Throw an exception instead.
            if (!overwrite)
            {
                try
                {
                    var existing = await destinationFolder.GetFirstByNameAsync(newName, cancellationToken);
                    if (existing is not null)
                        throw new FileAlreadyExistsException(newName);
                }
                catch (FileNotFoundException) { }
            }

            // If the destination folder declares a non-fallback move path, try that.
            // Provide a fallback in case this file is not a handled type.
            if (destinationFolder is IMoveRenamedFrom fastPath)
                return await fastPath.MoveFromAsync(fileToMove, source, overwrite, newName, cancellationToken, fallback: MoveRenamedFromFallbackAsync);

            // Manual move. Slower, but covers all scenarios.
            return await MoveRenamedFromFallbackAsync(destinationFolder, fileToMove, source, overwrite, newName, cancellationToken);
        }

        /// <summary>
        /// Moves a file out of the source folder and into the destination folder. Returns the new file that resides in this folder.
        /// </summary>
        /// <remarks>
        /// If the destination folder does not implement <see cref="IMoveFrom"/>, copy and delete operations take precedence.
        /// The delete operation, if supported, deletes the item immediately, instead of recycling it.
        /// </remarks>
        /// <param name="destinationFolder">The folder where the file is moved to.</param>
        /// <param name="fileToMove">The file being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="fileToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination file can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <exception cref="FileAlreadyExistsException">Thrown when <paramref name="overwrite"/> is false and the resource being created already exists.</exception>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the moved file.</returns>
        public static async Task<IChildFile> MoveFileImmediatelyFrom(this IModifiableFolder destinationFolder, IChildFile fileToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If the destination file exists and overwrite is false, it shouldn't be overwritten or returned as-is. Throw an exception instead.
            if (!overwrite)
            {
                try
                {
                    var existing = await destinationFolder.GetFirstByNameAsync(fileToMove.Name, cancellationToken);
                    if (existing is not null)
                        throw new FileAlreadyExistsException(fileToMove.Name);
                }
                catch (FileNotFoundException) { }
            }

            // If the destination folder declares a non-fallback move path, try that.
            // Provide a fallback in case this file is not a handled type.
            if (destinationFolder is IMoveFrom fastPath)
                return await fastPath.MoveFromAsync(fileToMove, source, overwrite, cancellationToken, fallback: MoveFromFallbackAsync);

            // Manual move. Slower, but covers all scenarios.
            return await MoveFromFallbackAsync(destinationFolder, fileToMove, source, overwrite, cancellationToken);
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the moved folder.</returns>
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
                            var movedFile = await currentDestination.MoveFileImmediatelyFrom(file, currentFolder, overwrite, cancellationToken);
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

        private static async Task<IChildFile> MoveRenamedFromFallbackAsync(IModifiableFolder destinationFolder, IChildFile fileToMove, IModifiableFolder source, bool overwrite, string newName, CancellationToken cancellationToken = default)
        {
            // Use the copy fallback directly
            var file = await destinationFolder.CreateCopyOfAsync(fileToMove, overwrite, newName, cancellationToken);

            // Delete the source file
            await source.DeleteAsync(fileToMove, -1L, true, cancellationToken);

            return file;
        }

        private static async Task<IChildFile> MoveFromFallbackAsync(IModifiableFolder destinationFolder, IChildFile fileToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
        {
            // Use the copy fallback directly
            var file = await destinationFolder.CreateCopyOfAsync(fileToMove, overwrite, fileToMove.Name, cancellationToken);

            // Delete the source file
            await source.DeleteAsync(fileToMove, -1L, true, cancellationToken);

            return file;
        }
    }
}
