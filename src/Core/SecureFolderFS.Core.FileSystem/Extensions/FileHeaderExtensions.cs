using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Extensions
{
    public static class FileHeaderExtensions
    {
        [SkipLocalsInit]
        public static bool ReadHeader(this HeaderBuffer headerBuffer, Stream ciphertextStream, IHeaderCrypt headerCrypt)
        {
            if (headerBuffer.IsHeaderReady)
                return true;

            if (!ciphertextStream.CanRead)
                throw FileSystemExceptions.StreamNotReadable;

            // The header buffer is shared by all streams of the same file, so a lock is needed
            headerBuffer.SyncRoot.Wait();
            try
            {
                // Re-check after lock
                if (headerBuffer.IsHeaderReady)
                    return true;

                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[headerCrypt.HeaderCiphertextSize];

                // Read header
                int read;
                if (ciphertextStream.CanSeek && ciphertextStream.Position != 0L)
                {
                    var ciphertextPosition = ciphertextStream.Position;
                    ciphertextStream.Position = 0L;

                    read = ciphertextStream.Read(ciphertextHeader);
                    ciphertextStream.Position = ciphertextPosition;
                }
                else
                {
                    // Non-seekable streams must be at position 0 - header is always read first sequentially.
                    // There is no way to rewind, so we simply read and continue.
                    read = ciphertextStream.Read(ciphertextHeader);
                }

                // Check if the read amount is correct
                if (read < ciphertextHeader.Length)
                    return false;

                // Decrypt header
                headerBuffer.IsHeaderReady = headerCrypt.DecryptHeader(ciphertextHeader, headerBuffer);

                return headerBuffer.IsHeaderReady;
            }
            finally
            {
                headerBuffer.SyncRoot.Release();
            }
        }

        /// <inheritdoc cref="ReadHeader"/>
        public static async ValueTask<bool> ReadHeaderAsync(this HeaderBuffer headerBuffer, Stream ciphertextStream, IHeaderCrypt headerCrypt, CancellationToken cancellationToken = default)
        {
            if (headerBuffer.IsHeaderReady)
                return true;

            if (!ciphertextStream.CanRead)
                throw FileSystemExceptions.StreamNotReadable;

            // The header buffer is shared by all streams of the same file, so a lock is needed
            await headerBuffer.SyncRoot.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Re-check after lock
                if (headerBuffer.IsHeaderReady)
                    return true;

                // Rent ciphertext header buffer (asynchronous methods cannot use stackalloc)
                var ciphertextHeader = ArrayPool<byte>.Shared.Rent(headerCrypt.HeaderCiphertextSize);
                try
                {
                    // ArrayPool may return a larger array than requested
                    var realCiphertextHeader = ciphertextHeader.AsMemory(0, headerCrypt.HeaderCiphertextSize);

                    // Read header
                    int read;
                    if (ciphertextStream.CanSeek && ciphertextStream.Position != 0L)
                    {
                        var ciphertextPosition = ciphertextStream.Position;
                        ciphertextStream.Position = 0L;

                        read = await ciphertextStream.ReadAsync(realCiphertextHeader, cancellationToken).ConfigureAwait(false);
                        ciphertextStream.Position = ciphertextPosition;
                    }
                    else
                    {
                        // Non-seekable streams must be at position 0 - header is always read first sequentially.
                        // There is no way to rewind, so we simply read and continue.
                        read = await ciphertextStream.ReadAsync(realCiphertextHeader, cancellationToken).ConfigureAwait(false);
                    }

                    // Check if the read amount is correct
                    if (read < realCiphertextHeader.Length)
                        return false;

                    // Decrypt header
                    headerBuffer.IsHeaderReady = headerCrypt.DecryptHeader(realCiphertextHeader.Span, headerBuffer);

                    return headerBuffer.IsHeaderReady;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(ciphertextHeader);
                }
            }
            finally
            {
                headerBuffer.SyncRoot.Release();
            }
        }
    }
}
