using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <returns>If file was found, returns the requested <see cref="IFile"/>, otherwise null.</returns>
        /// <inheritdoc cref="IFolder.GetFileAsync"/>
        public static async Task<IFile?> TryGetFileAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.GetFileAsync(fileName, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <returns>If folder was found, returns the requested <see cref="IFolder"/>, otherwise null.</returns>
        /// <inheritdoc cref="IFolder.GetFileAsync"/>
        public static async Task<IFolder?> TryGetFolderAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.GetFolderAsync(folderName, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <returns>If file was created, returns the requested <see cref="IFile"/>, otherwise null.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFileAsync"/>
        public static async Task<IFile?> TryCreateFileAsync(this IModifiableFolder folder, string desiredName, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.CreateFileAsync(desiredName, collisionOption, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <returns>If folder was created, returns the requested <see cref="IFolder"/>, otherwise null.</returns>
        /// <inheritdoc cref="IModifiableFolder.CreateFolderAsync"/>
        public static async Task<IFolder?> TryCreateFolderAsync(this IModifiableFolder folder, string desiredName, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.CreateFolderAsync(desiredName, collisionOption, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
