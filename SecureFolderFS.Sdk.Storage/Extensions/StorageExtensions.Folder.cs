using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        #region Extension Methods

        /// <inheritdoc cref="IFolderExtended.GetFileAsync"/>
        public static async Task<IFile> GetFileAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            if (folder is IFolderExtended folderExtended)
                return await folderExtended.GetFileAsync(fileName, cancellationToken);

            await foreach (var item in folder.GetFilesAsync(cancellationToken))
            {
                if (item.Name == fileName)
                    return item;
            }

            throw new FileNotFoundException();
        }

        /// <inheritdoc cref="IFolderExtended.GetFolderAsync"/>
        public static async Task<IFolder> GetFolderAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            if (folder is IFolderExtended folderExtended)
                return await folderExtended.GetFolderAsync(folderName, cancellationToken);

            await foreach (var item in folder.GetFoldersAsync(cancellationToken))
            {
                if (item.Name == folderName)
                    return item;
            }

            throw new DirectoryNotFoundException();
        }

        #endregion

        #region Try Methods

        /// <returns>If file was found, returns the requested <see cref="IFile"/>, otherwise null.</returns>
        /// <inheritdoc cref="IFolderExtended.GetFileAsync"/>
        public static async Task<IFile?> TryGetFileAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetFileAsync(folder, fileName, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <returns>If folder was found, returns the requested <see cref="IFolder"/>, otherwise null.</returns>
        /// <inheritdoc cref="IFolderExtended.GetFolderAsync"/>
        public static async Task<IFolder?> TryGetFolderAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetFolderAsync(folder, folderName, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <returns>If file was created, returns the requested <see cref="IFile"/>, otherwise null.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFileAsync"/>
        public static async Task<IFile?> TryCreateFileAsync(this IModifiableFolder folder, string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.CreateFileAsync(desiredName, overwrite, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <returns>If folder was created, returns the requested <see cref="IFolder"/>, otherwise null.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFolderAsync"/>
        public static async Task<IFolder?> TryCreateFolderAsync(this IModifiableFolder folder, string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.CreateFolderAsync(desiredName, overwrite, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Try with Result

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the file was found or not.</returns>
        /// <inheritdoc cref="IFolderExtended.GetFileAsync"/>
        public static async Task<IResult<IFile?>> GetFileWithResultAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                var file = await GetFileAsync(folder, fileName, cancellationToken);
                return new CommonResult<IFile?>(file);
            }
            catch (Exception ex)
            {
                return new CommonResult<IFile?>(ex);
            }
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the folder was found or not.</returns>
        /// <inheritdoc cref="IFolderExtended.GetFolderAsync"/>
        public static async Task<IResult<IFolder?>> GetFolderWithResultAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            try
            {
                var folder2 = await GetFolderAsync(folder, folderName, cancellationToken);
                return new CommonResult<IFolder?>(folder2);
            }
            catch (Exception ex)
            {
                return new CommonResult<IFolder?>(ex);
            }
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the file was created successfully.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFileAsync"/>
        public static async Task<IResult<IFile?>> CreateFileWithResultAsync(this IModifiableFolder folder, string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            try
            {
                var file = await folder.CreateFileAsync(desiredName, overwrite, cancellationToken);
                return new CommonResult<IFile?>(file);
            }
            catch (Exception ex)
            {
                return new CommonResult<IFile?>(ex);
            }
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the folder was created successfully.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFolderAsync"/>
        public static async Task<IResult<IFolder?>> CreateFolderWithResultAsync(this IModifiableFolder folder, string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            try
            {
                var folder2 = await folder.CreateFolderAsync(desiredName, overwrite, cancellationToken);
                return new CommonResult<IFolder?>(folder2);
            }
            catch (Exception ex)
            {
                return new CommonResult<IFolder?>(ex);
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// Gets all files contained within <paramref name="folder"/>.
        /// </summary>
        /// <param name="folder">The folder to enumerate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFile"/> of files in the directory.</returns>
        public static async IAsyncEnumerable<IFile> GetFilesAsync(this IFolder folder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in folder.GetItemsAsync(StorableKind.Files, cancellationToken))
            {
                if (item is IFile fileItem)
                    yield return fileItem;
            }
        }

        /// <summary>
        /// Gets all folders contained within <paramref name="folder"/>.
        /// </summary>
        /// <param name="folder">The folder to enumerate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFolder"/> of folders in the directory.</returns>
        public static async IAsyncEnumerable<IFolder> GetFoldersAsync(this IFolder folder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in folder.GetItemsAsync(StorableKind.Files, cancellationToken))
            {
                if (item is IFolder folderItem)
                    yield return folderItem;
            }
        }

        #endregion
    }
}
