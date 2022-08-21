using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Shared.Extensions;
using System;
using static SecureFolderFS.Core.Constants.Security.Chunks.XChaCha20Poly1305;
using static SecureFolderFS.Core.Constants.Security.Headers.XChaCha20Poly1305;
using static SecureFolderFS.Core.Extensions.SecurityExtensions.Content.XChaChaContentExtensions;
using static SecureFolderFS.Core.Extensions.SecurityExtensions.Header.XChaChaHeaderExtensions;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal sealed class XChaChaContentCrypt : BaseContentCrypt
    {
        /// <inheritdoc/>
        public override int ChunkCleartextSize { get; } = CHUNK_CLEARTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkCiphertextSize { get; } = CHUNK_CIPHERTEXT_SIZE;

        public XChaChaContentCrypt(ICipherProvider cipherProvider)
            : base(cipherProvider)
        {
        }

        /// <inheritdoc/>
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

            // Encrypt
            cipherProvider.XChaCha20Poly1305Crypt.Encrypt(
                cleartextChunk,
                header.GetHeaderContentKey(),
                ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE),
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE),
                beChunkNumberWithFileHeaderNonce
            );
        }

        /// <inheritdoc/>
        public override bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> cleartextChunk)
        {
            // Big Endian chunk number and file header nonce
            // TODO: OPTIMIZE
            var beChunkNumberWithFileHeaderNonce = new byte[sizeof(long) + HEADER_NONCE_SIZE];
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();
            Buffer.BlockCopy(beChunkNumber, 0, beChunkNumberWithFileHeaderNonce, 0, beChunkNumber.Length);
            Buffer.BlockCopy(header.GetHeaderNonce().ToArray(), 0, beChunkNumberWithFileHeaderNonce, beChunkNumber.Length, HEADER_NONCE_SIZE);

            // Decrypt
            return cipherProvider.XChaCha20Poly1305Crypt.Decrypt(
                ciphertextChunk.GetChunkPayloadWithTag(),
                header.GetHeaderContentKey(),
                ciphertextChunk.GetChunkNonce(),
                cleartextChunk.Slice(0, ciphertextChunk.Length - (CHUNK_NONCE_SIZE + CHUNK_TAG_SIZE)),
                beChunkNumberWithFileHeaderNonce);
        }
    }
}
