using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <summary>
        /// Copies contents of <paramref name="source"/> to <paramref name="destination"/> overwriting existing data.
        /// </summary>
        /// <param name="source">The source file to copy from.</param>
        /// <param name="destination">The destination file to copy to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task CopyContentsToAsync(this IFile source, IFile destination, CancellationToken cancellationToken = default)
        {
            await using var sourceStream = await source.OpenStreamAsync(FileAccess.Read, cancellationToken);
            await using var destinationStream = await destination.OpenStreamAsync(FileAccess.Read, cancellationToken);
            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        }

        /// <summary>
        /// Writes <paramref name="text"/> into specified <paramref name="file"/> overwriting existing content.
        /// </summary>
        /// <param name="file">The file to write to.</param>
        /// <param name="text">The text to write.</param>
        /// <param name="encoding">The encoding to use when writing. Default is <see cref="Encoding.UTF8"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task WriteAllTextAsync(this IFile file, string text, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            await using var fileStream = await file.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);

            // Reset the stream
            fileStream.SetLength(0L);
            fileStream.Seek(0L, SeekOrigin.Begin);

            await using var streamWriter = new StreamWriter(fileStream, encoding ?? Encoding.UTF8);
            await streamWriter.WriteAsync(text.AsMemory(), cancellationToken);
        }

        /// <summary>
        /// Reads all text contents from specified <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to read from.</param>
        /// <param name="encoding">The encoding to use when reading. Default is <see cref="Encoding.UTF8"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="string"/> that contains text found in the file.</returns>
        public static async Task<string> ReadAllTextAsync(this IFile file, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            await using var fileStream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);

            using var streamReader = new StreamReader(fileStream, encoding ?? Encoding.UTF8);
            return await streamReader.ReadToEndAsync(cancellationToken);
        }
    }
}
