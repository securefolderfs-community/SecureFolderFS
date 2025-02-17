using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <summary>
        /// Creates a copy of the provided storable within this folder.
        /// </summary>
        /// <typeparam name="TStorable">The type of storable, whether a <see cref="IFile"/> or a <see cref="IFolder"/>.</typeparam>
        /// <param name="destinationFolder">The folder where the copy is created.</param>
        /// <param name="itemToCopy">The storable to be copied into this folder.</param>
        /// <param name="overwrite"><code>true</code> if any existing destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<TStorable> CreateCopyOfStorableAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToCopy, bool overwrite = false, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            return itemToCopy switch
            {
                IFile fileToCopy => (TStorable)await destinationFolder.CreateCopyOfAsync(fileToCopy, overwrite, cancellationToken),
                IFolder folderToCopy => (TStorable)await destinationFolder.CreateCopyOfAsync(folderToCopy, overwrite, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(itemToCopy))
            };
        }
        
        /// <summary>
        /// Moves a storable from the source folder into the destination folder. Returns the new storable that resides in the destination folder.
        /// </summary>
        /// <typeparam name="TStorable">The type of storable, whether a <see cref="IFile"/> or a <see cref="IFolder"/>.</typeparam>
        /// <param name="destinationFolder">The folder where the storable is moved to.</param>
        /// <param name="itemToMove">The storable being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="itemToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<TStorable> MoveStorableFromAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
            where TStorable : IStorableChild
        {
            return itemToMove switch
            {
                IChildFile fileToMove => (TStorable)await destinationFolder.MoveFromAsync(fileToMove, source, overwrite, cancellationToken),
                IChildFolder folderToMove => (TStorable)await destinationFolder.MoveFromAsync(folderToMove, source, overwrite, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(itemToMove))
            };
        }        
        
        /// <summary>
        /// Creates a copy of the provided folder within this folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the copy is created.</param>
        /// <param name="folderToCopy">The folder to be copied into this folder.</param>
        /// <param name="overwrite"><code>true</code> if any existing destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<IModifiableFolder> CreateCopyOfAsync(this IModifiableFolder destinationFolder, IFolder folderToCopy, bool overwrite, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create the corresponding folder in the destination
            var copiedFolder = (IModifiableFolder)await destinationFolder.CreateFolderAsync(folderToCopy.Name, overwrite, cancellationToken);

            // Iterate through all items in the source folder
            await foreach (var item in folderToCopy.GetItemsAsync(StorableType.All, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (item)
                {
                    case IFile file:
                    {
                        // Copy the file to the destination folder
                        await copiedFolder.CreateCopyOfAsync(file, overwrite, cancellationToken);
                        break;
                    }
                    
                    case IFolder subFolder:
                    {
                        // Recursively copy the subfolder
                        await copiedFolder.CreateCopyOfAsync(subFolder, overwrite, cancellationToken);
                        break;
                    }
                }
            }

            return copiedFolder;
        }

        /// <summary>
        /// Moves a folder from the source folder into the destination folder. Returns the new folder that resides in the destination folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the folder is moved to.</param>
        /// <param name="folderToMove">The folder being moved into this folder.</param>
        /// <param name="source">The folder that <paramref name="folderToMove"/> is being moved from.</param>
        /// <param name="overwrite"><code>true</code> if the destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<IModifiableFolder> MoveFromAsync(this IModifiableFolder destinationFolder, IChildFolder folderToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
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

            // If the destination folder supports optimized moving, use it
            // if (destinationFolder is IMoveFrom fastPath)
            //     return await fastPath.MoveFromAsync(folderToMove, source, overwrite, cancellationToken,
            //         fallback: MoveFromFallbackAsync);

            // Perform a manual move (fallback)
            return await MoveFromFallbackAsync(destinationFolder, folderToMove, source, overwrite, cancellationToken);

            async Task<IModifiableFolder> MoveFromFallbackAsync(IModifiableFolder destinationFolder, IChildFolder folderToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken = default)
            {
                // Create a copy of the folder in the destination
                var movedFolder = await destinationFolder.CreateCopyOfAsync(folderToMove, overwrite, cancellationToken);

                // Delete the original folder
                await source.DeleteAsync(folderToMove, cancellationToken);

                return movedFolder;
            }
        }
        
        /// <summary>
        /// Gets a file in the current directory by name.
        /// </summary>
        /// <param name="folder">The folder to get items from.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IChildFile"/>; otherwise an exception is thrown.</returns>
        public static async Task<IChildFile> GetFileByNameAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            return await folder.GetFirstByNameAsync(fileName, cancellationToken) switch
            {
                IChildFile childFile => childFile,
                _ => throw new InvalidOperationException("The provided name does not point to a file.")
            };
        }

        /// <summary>
        /// Gets a folder in the current directory by name.
        /// </summary>
        /// <param name="folder">The folder to get items from.</param>
        /// <param name="folderName">The name of the folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IChildFolder"/>; otherwise an exception is thrown.</returns>
        public static async Task<IChildFolder> GetFolderByNameAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            return await folder.GetFirstByNameAsync(folderName, cancellationToken) switch
            {
                IChildFolder childFolder => childFolder,
                _ => throw new InvalidOperationException("The provided name does not point to a folder.")
            };
        }

        /// <summary>
        /// From the provided <see cref="IStorable"/>, traverses the provided relative or the same matching path and returns the item at that path.
        /// </summary>
        /// <param name="from">The item to start with when traversing.</param>
        /// <param name="relativePath">The path of the storable item to return, relative to the provided item.</param>
        /// <param name="cancellationToken">A token to cancel the ongoing operation.</param>
        /// <returns>The <see cref="IStorable"/> item found at the relative path.</returns>
        /// <exception cref="ArgumentException">
        /// A parent directory was specified, but the provided <see cref="IStorable"/> is not addressable.
        /// Or, the provided relative path named a folder, but the item was a file.
        /// Or, an empty path part was found.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">A parent folder was requested, but the storable item did not return a parent.</exception>
        /// <exception cref="FileNotFoundException">A named item was specified in a folder, but the item wasn't found.</exception>
        public static async Task<IStorable> GetItemByRelativePathOrSelfAsync(this IStorable from, string relativePath, CancellationToken cancellationToken = default)
        {
            return from.Id == relativePath? from : await from.GetItemByRelativePathAsync(relativePath, cancellationToken);
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the file was created successfully.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFileAsync"/>
        public static async Task<IResult<IFile?>> CreateFileWithResultAsync(this IModifiableFolder folder, string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            try
            {
                var file = await folder.CreateFileAsync(desiredName, overwrite, cancellationToken);
                return Result<IFile?>.Success(file);
            }
            catch (Exception ex)
            {
                return Result<IFile?>.Failure(ex);
            }
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the folder was created successfully.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFolderAsync"/>
        public static async Task<IResult<IFolder?>> CreateFolderWithResultAsync(this IModifiableFolder folder, string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            try
            {
                var folder2 = await folder.CreateFolderAsync(desiredName, overwrite, cancellationToken);
                return Result<IFolder?>.Success(folder2);
            }
            catch (Exception ex)
            {
                return Result<IFolder?>.Failure(ex);
            }
        }
    }
}
