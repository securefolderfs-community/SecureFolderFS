using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Chunks.AesGcm;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Headers.AesGcm;
using static SecureFolderFS.Core.Cryptography.Extensions.ContentCryptExtensions.AesGcmContentExtensions;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.AesGcmHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.ContentCrypt
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal sealed class AesGcmContentCrypt : BaseContentCrypt
    {
        /// <inheritdoc/>
        public override int ChunkPlaintextSize { get; } = CHUNK_PLAINTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkCiphertextSize { get; } = CHUNK_CIPHERTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkFirstReservedSize { get; } = CHUNK_NONCE_SIZE;

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override void EncryptChunk(ReadOnlySpan<byte> plaintextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk)
        {
            // Chunk nonce
            RandomNumberGenerator.Fill(ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE));

            // Big Endian chunk number and file header nonce
            Span<byte> associatedData = stackalloc byte[sizeof(long) + HEADER_NONCE_SIZE];
            CryptHelpers.FillAssociatedDataBe(associatedData, header.GetHeaderNonce(), chunkNumber);

            // Encrypt
            AesGcm128.Encrypt(
                plaintextChunk,
                header.GetHeaderContentKey(),
                ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE),
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE + plaintextChunk.Length),
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE, plaintextChunk.Length),
                associatedData);
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> plaintextChunk)
        {
            // Big Endian chunk number and file header nonce
            Span<byte> associatedData = stackalloc byte[sizeof(long) + HEADER_NONCE_SIZE];
            CryptHelpers.FillAssociatedDataBe(associatedData, header.GetHeaderNonce(), chunkNumber);

            // Decrypt
            return AesGcm128.TryDecrypt(
                ciphertextChunk.GetChunkPayload(),
                header.GetHeaderContentKey(),
                ciphertextChunk.GetChunkNonce(),
                ciphertextChunk.GetChunkTag(),
                plaintextChunk.Slice(0, ciphertextChunk.Length - (CHUNK_NONCE_SIZE + CHUNK_TAG_SIZE)),
                associatedData);
        }
    }
}
