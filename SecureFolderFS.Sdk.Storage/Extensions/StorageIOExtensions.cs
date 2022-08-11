using System;
using System.IO;
using System.Text;
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

        public static async Task<string?> ReadStringAsync(IFile file, Encoding encoding, CancellationToken cancellationToken = default)
        {
            await using var stream = await file.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            if (stream is null)
                return null;

            var buffer = new byte[stream.Length];
            var read = await stream.ReadAsync(buffer, cancellationToken);

            return encoding.GetString(buffer.AsSpan(0, read));
        }
    }
}
