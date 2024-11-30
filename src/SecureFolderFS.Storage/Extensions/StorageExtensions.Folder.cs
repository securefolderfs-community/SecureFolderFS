using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
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
