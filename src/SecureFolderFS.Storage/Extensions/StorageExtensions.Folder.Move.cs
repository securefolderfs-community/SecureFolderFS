using System;
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
                IChildFolder folderToMove => (TStorable)await destinationFolder.MoveFromAsync(folderToMove, source, overwrite, reporter, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(itemToMove))
            };
        }

        /// <inheritdoc cref="MoveFromAsync(IModifiableFolder,IChildFolder,IModifiableFolder,bool,IProgress{IStorable},CancellationToken)"/>
        public static Task<IModifiableFolder> MoveFromAsync(this IModifiableFolder destinationFolder, IChildFolder folderToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
        {
            return MoveFromAsync(destinationFolder, folderToMove, source, overwrite, null, cancellationToken);
        }

        /// <summary>
        /// Moves a folder from the source folder into the destination folder. Returns the new folder that resides in the destination folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the folder is moved to.</param>
        /// <param name="folderToMove">The folder being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="folderToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<IModifiableFolder> MoveFromAsync(this IModifiableFolder destinationFolder, IChildFolder folderToMove, IModifiableFolder source, bool overwrite, IProgress<IStorable>? reporter = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check if a folder with the same name already exists in the destination
            if (!overwrite)
            {
                try
                {
                    var existing = await destinationFolder.GetFirstByNameAsync(folderToMove.Name, cancellationToken);
                    if (existing is not null)
                        throw new FileAlreadyExistsException(folderToMove.Name);
                }
                catch (FileNotFoundException)
                {
                }
                catch (DirectoryNotFoundException)
                {
                }
            }

            // TODO: If the destination folder supports optimized moving, use it
            // if (destinationFolder is IMoveFrom fastPath)
            //     return await fastPath.MoveFromAsync(folderToMove, source, overwrite, cancellationToken,
            //         fallback: MoveFromFallbackAsync);

            // Perform a manual move (fallback)
            return await MoveFromFallbackAsync(destinationFolder, folderToMove, source, overwrite, reporter, cancellationToken);

            async Task<IModifiableFolder> MoveFromFallbackAsync(IModifiableFolder destinationFolder, IChildFolder folderToMove, IModifiableFolder source, bool overwrite, IProgress<IStorable>? reporter = null, CancellationToken cancellationToken = default)
            {
                // Create a copy of the folder in the destination
                var movedFolder = await destinationFolder.CreateCopyOfAsync(folderToMove, overwrite, reporter, cancellationToken);

                // Delete the original folder
                await source.DeleteAsync(folderToMove, cancellationToken);

                return movedFolder;
            }
        }
    }
}
