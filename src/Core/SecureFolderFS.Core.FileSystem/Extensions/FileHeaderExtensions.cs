using System;
using System.IO;
using System.Runtime.CompilerServices;
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
            lock (headerBuffer.SyncRoot)
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
        }
    }
}