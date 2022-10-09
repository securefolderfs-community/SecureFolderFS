using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the file was found or not.</returns>
        /// <inheritdoc cref="IFolder.GetFileAsync"/>
        public static async Task<IResult<IFile?>> GetFileWithResultAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                return new CommonResult<IFile?>(await folder.GetFileAsync(fileName, cancellationToken));
            }
            catch (Exception ex)
            {
                return new CommonResult<IFile?>(ex);
            }
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the folder was found or not.</returns>
        /// <inheritdoc cref="IFolder.GetFileAsync"/>
        public static async Task<IResult<IFolder?>> GetFolderWithResultAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            try
            {
                return new CommonResult<IFolder?>(await folder.GetFolderAsync(folderName, cancellationToken));
            }
            catch (Exception ex)
            {
                return new CommonResult<IFolder?>(ex);
            }
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the file was created successfully.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFileAsync"/>
        public static async Task<IResult<IFile?>> CreateFileWithResultAsync(this IModifiableFolder folder, string desiredName, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            try
            {
                return new CommonResult<IFile?>(await folder.CreateFileAsync(desiredName, collisionOption, cancellationToken));
            }
            catch (Exception ex)
            {
                return new CommonResult<IFile?>(ex);
            }
        }

        /// <returns>Value is <see cref="IResult{T}"/> depending on whether the folder was created successfully.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFolderAsync"/>
        public static async Task<IResult<IFolder?>> CreateFolderWithResultAsync(this IModifiableFolder folder, string desiredName, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            try
            {
                return new CommonResult<IFolder?>(await folder.CreateFolderAsync(desiredName, collisionOption, cancellationToken));
            }
            catch (Exception ex)
            {
                return new CommonResult<IFolder?>(ex);
            }
        }
    }
}
