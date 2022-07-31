using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static class StorageExtensions
    {
        public static async Task<IFile> CopyToAsync(this IFile source, IModifiableFolder destination, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            return (IFile)await destination.CreateCopyOfAsync(source, collisionOption, cancellationToken);
        }

        public static async Task<IFolder> CopyToAsync(this IFolder source, IModifiableFolder destination, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            return (IFolder)await destination.CreateCopyOfAsync(source, collisionOption, cancellationToken);
        }

        public static async Task<Stream?> TryOpenStreamAsync(this IFile file, FileAccess access, CancellationToken cancellationToken = default)
        {
            try
            {
                return await file.OpenStreamAsync(access, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task CopyContentsToAsync(this IFile source, IFile destination)
        {
            await using var sourceStream = await source.OpenStreamAsync(FileAccess.Read);
            await using var destinationStream = await destination.OpenStreamAsync(FileAccess.Read);

            await sourceStream.CopyToAsync(destinationStream);
        }
    }
}
