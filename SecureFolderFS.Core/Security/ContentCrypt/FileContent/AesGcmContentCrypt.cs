using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Constants.Security.Chunks.AesGcm;
using static SecureFolderFS.Core.Constants.Security.Headers.AesGcm;
using static SecureFolderFS.Core.Extensions.SecurityExtensions.Content.AesGcmContentExtensions;
using static SecureFolderFS.Core.Extensions.SecurityExtensions.Header.AesGcmHeaderExtensions;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal sealed class AesGcmContentCrypt : BaseContentCrypt
    {
        /// <inheritdoc/>
        public override int ChunkCleartextSize { get; } = CHUNK_CLEARTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkCiphertextSize { get; } = CHUNK_CIPHERTEXT_SIZE;

        public AesGcmContentCrypt(ICipherProvider cipherProvider)
            : base(cipherProvider)
        {
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override void EncryptChunk(ReadOnlySpan<byte> cleartextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk)
        {
            // Chunk nonce
            secureRandom.GetBytes(ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE));

            // Big Endian chunk number and file header nonce
            // TODO: OPTIMIZE
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();
            var beChunkNumberWithFileHeaderNonce = new byte[sizeof(long) + HEADER_NONCE_SIZE];
            Buffer.BlockCopy(beChunkNumber, 0, beChunkNumberWithFileHeaderNonce, 0, beChunkNumber.Length);
            Buffer.BlockCopy(header.GetHeaderNonce().ToArray(), 0, beChunkNumberWithFileHeaderNonce, beChunkNumber.Length, HEADER_NONCE_SIZE);

            cipherProvider.AesGcmCrypt.Encrypt(
                cleartextChunk,
                header.GetHeaderContentKey(),
                ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE),
                ciphertextChunk.Slice(CHUNK_TAG_SIZE + cleartextChunk.Length),
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE, cleartextChunk.Length),
                beChunkNumberWithFileHeaderNonce);
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> cleartextChunk)
        {
            // Big Endian chunk number and file header nonce
            // TODO: OPTIMIZE
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();
            var beChunkNumberWithFileHeaderNonce = new byte[sizeof(long) + HEADER_NONCE_SIZE];
            Buffer.BlockCopy(beChunkNumber, 0, beChunkNumberWithFileHeaderNonce, 0, beChunkNumber.Length);
            Buffer.BlockCopy(header.GetHeaderNonce().ToArray(), 0, beChunkNumberWithFileHeaderNonce, beChunkNumber.Length, HEADER_NONCE_SIZE);

            // Decrypt
            return cipherProvider.AesGcmCrypt.Decrypt(
                ciphertextChunk.GetChunkPayload(),
                header.GetHeaderContentKey(),
                ciphertextChunk.GetChunkNonce(),
                ciphertextChunk.GetChunkTag(),
                cleartextChunk.Slice(0, ciphertextChunk.Length - (CHUNK_NONCE_SIZE + CHUNK_TAG_SIZE)),
                beChunkNumberWithFileHeaderNonce);
        }
    }
}
