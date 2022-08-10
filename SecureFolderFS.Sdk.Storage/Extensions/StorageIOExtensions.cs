using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static class StorageIOExtensions
    {
        public static async Task CopyContentsToAsync(this IFile source, IFile destination, CancellationToken cancellationToken = default)
        {
            await using var sourceStream = await source.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            await using var destinationStream = await destination.OpenStreamAsync(FileAccess.Read, FileShare.None, cancellationToken);

            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        }
    }
}
