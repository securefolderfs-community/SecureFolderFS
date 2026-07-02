using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Shared.ComponentModel;
using System.IO;

#pragma warning disable CA1416 // Callsite is not supported on all platforms

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <inheritdoc cref="FileHandle"/>
    internal sealed class DokanyFileHandle : FileHandle
    {
        private readonly Security _security;

        public DokanyFileHandle(Stream stream, Security security)
            : base(stream)
        {
            _security = security;
        }

        /// <summary>
        /// Sets file time for this file handle.
        /// </summary>
        /// <param name="creationTime">Creation time to set.</param>
        /// <param name="lastAccessTime">Last access time to set.</param>
        /// <param name="lastWriteTime">Last write time to set.</param>
        /// <returns>If the time was set successfully, returns true; otherwise false.</returns>
        public bool SetFileTime(ref long creationTime, ref long lastAccessTime, ref long lastWriteTime)
        {
            var hFile = GetHandle();
            if (hFile is null)
                return false;

            return UnsafeNativeApis.SetFileTime(hFile, ref creationTime, ref lastAccessTime, ref lastWriteTime);

            SafeFileHandle? GetHandle()
            {
                return Stream switch
                {
                    IWrapper<Stream> { Inner: FileStream fileStream } => fileStream.SafeFileHandle,
                    FileStream fileStream2 => fileStream2.SafeFileHandle,
                    _ => null
                };
            }
        }

        /// <inheritdoc cref="FileStream.Lock"/>
        public void Lock(long position, long length)
        {
            if (Stream is IWrapper<Stream> { Inner: FileStream fileStream })
            {
                var (ciphertextPosition, ciphertextLength) = TranslateToCiphertextRange(position, length);
                fileStream.Lock(ciphertextPosition, ciphertextLength);
            }
        }

        /// <inheritdoc cref="FileStream.Unlock"/>
        public void Unlock(long position, long length)
        {
            if (Stream is IWrapper<Stream> { Inner: FileStream fileStream })
            {
                var (ciphertextPosition, ciphertextLength) = TranslateToCiphertextRange(position, length);
                fileStream.Unlock(ciphertextPosition, ciphertextLength);
            }
        }

        /// <summary>
        /// Translates a plaintext byte range to the ciphertext range covering the affected chunks.
        /// </summary>
        /// <remarks>
        /// Plaintext offsets cannot be applied to the ciphertext file directly as the file starts
        /// with the header, and each chunk carries additional overhead. Locking untranslated
        /// offsets would lock the header (offset 0) and misalign every subsequent range.
        /// The translation is deterministic, so a matching <see cref="Unlock"/> releases the exact same range.
        /// </remarks>
        private (long Position, long Length) TranslateToCiphertextRange(long plaintextPosition, long plaintextLength)
        {
            var chunkPlaintextSize = (long)_security.ContentCrypt.ChunkPlaintextSize;
            var chunkCiphertextSize = (long)_security.ContentCrypt.ChunkCiphertextSize;
            var headerCiphertextSize = (long)_security.HeaderCrypt.HeaderCiphertextSize;

            // Always cover at least the chunk containing the start position
            var firstChunk = plaintextPosition / chunkPlaintextSize;
            var lastChunkExclusive = plaintextLength <= 0L
                ? firstChunk + 1L
                : (plaintextPosition + plaintextLength + chunkPlaintextSize - 1L) / chunkPlaintextSize;

            var ciphertextPosition = headerCiphertextSize + firstChunk * chunkCiphertextSize;
            var ciphertextLength = (lastChunkExclusive - firstChunk) * chunkCiphertextSize;

            return (ciphertextPosition, ciphertextLength);
        }
    }
}
