using System;
using System.IO;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Extensions
{
    internal static class FileHeaderExtensions
    {
        [SkipLocalsInit]
        public static bool ReadHeader(this HeaderBuffer headerBuffer, Stream ciphertextStream, Security security)
        {
            if (headerBuffer.IsHeaderReady)
                return true;

            if (!ciphertextStream.CanRead)
                throw FileSystemExceptions.StreamNotReadable;

            var ciphertextPosition = ciphertextStream.Position;
            if (ciphertextPosition != 0 && !ciphertextStream.CanSeek)
                throw FileSystemExceptions.StreamNotSeekable;

            // Allocate ciphertext header
            Span<byte> ciphertextHeader = stackalloc byte[security.HeaderCrypt.HeaderCiphertextSize];

            // Read header
            int read;
            if (ciphertextPosition != 0)
            {
                ciphertextStream.Position = 0L;
                read = ciphertextStream.Read(ciphertextHeader);
                ciphertextStream.Position = ciphertextPosition;
            }
            else
                read = ciphertextStream.Read(ciphertextHeader);

            // Check if the read amount is correct
            if (read < ciphertextHeader.Length)
                return false;

            // Decrypt header
            headerBuffer.IsHeaderReady = security.HeaderCrypt.DecryptHeader(ciphertextHeader, headerBuffer);

            return headerBuffer.IsHeaderReady;
        }
    }
}