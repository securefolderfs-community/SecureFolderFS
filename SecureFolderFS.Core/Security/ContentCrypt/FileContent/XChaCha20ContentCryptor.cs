using System;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Chunks.Implementation;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    internal sealed class XChaCha20ContentCryptor : BaseContentCryptor<XChaCha20FileHeader, CleartextXChaCha20Chunk, CiphertextXChaCha20Chunk>
    {
        public override int ChunkCleartextSize { get; } = CleartextXChaCha20Chunk.CHUNK_CLEARTEXT_SIZE;

        public override int ChunkFullCiphertextSize { get; } = CiphertextXChaCha20Chunk.CHUNK_FULL_CIPHERTEXT_SIZE;

        public XChaCha20ContentCryptor(IKeyCryptor keyCryptor, IChunkFactory chunkFactory)
            : base(keyCryptor, chunkFactory)
        {
        }

        protected override ICiphertextChunk EncryptChunk(CleartextXChaCha20Chunk cleartextChunk, long chunkNumber, XChaCha20FileHeader fileHeader)
        {
            // Chunk nonce
            var chunkNonce = new byte[CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE];
            secureRandom.GetBytes(chunkNonce);

            // Big Endian chunk number and file header nonce
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();
            var beChunkNumberWithFileHeaderNonce = new byte[beChunkNumber.Length + fileHeader.Nonce.Length];
            beChunkNumberWithFileHeaderNonce.EmplaceArrays(beChunkNumber, fileHeader.Nonce);

            // Payload
            var ciphertextPayload = keyCryptor.XChaCha20Poly1305Crypt.XChaCha20Poly1305Encrypt(
                cleartextChunk.ToArray(),
                fileHeader.ContentKey,
                chunkNonce,
                out var tag,
                beChunkNumberWithFileHeaderNonce);

            // Construct ciphertextChunkBuffer
            var ciphertextChunkBuffer = new byte[CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE + ciphertextPayload.Length + CiphertextXChaCha20Chunk.CHUNK_TAG_SIZE];
            ciphertextChunkBuffer.EmplaceArrays(chunkNonce, ciphertextPayload, tag);

            return chunkFactory.FromCiphertextChunkBuffer(ciphertextChunkBuffer);
        }

        protected override ICleartextChunk DecryptChunk(CiphertextXChaCha20Chunk ciphertextChunk, long chunkNumber, XChaCha20FileHeader fileHeader)
        {
            // Big Endian chunk number and file header nonce
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();
            var beChunkNumberWithFileHeaderNonce = new byte[beChunkNumber.Length + fileHeader.Nonce.Length];
            beChunkNumberWithFileHeaderNonce.EmplaceArrays(beChunkNumber, fileHeader.Nonce);

            var cleartextChunkBuffer = keyCryptor.XChaCha20Poly1305Crypt.XChaCha20Poly1305Decrypt(
                ciphertextChunk.Payload,
                fileHeader.ContentKey,
                ciphertextChunk.Nonce,
                ciphertextChunk.Auth,
                beChunkNumberWithFileHeaderNonce);

            if (cleartextChunkBuffer == null)
            {
                throw UnauthenticChunkException.ForXChaCha20();
            }

            return chunkFactory.FromCleartextChunkBuffer(ExtendCleartextChunkBuffer(cleartextChunkBuffer), cleartextChunkBuffer.Length);
        }

        protected override void CheckIntegrity(CiphertextXChaCha20Chunk ciphertextChunk, XChaCha20FileHeader fileHeader, long chunkNumber, bool requestedIntegrityCheck)
        {
            if (!requestedIntegrityCheck)
            {
                throw new InvalidOperationException($"In XChaCha20-Poly1305 encryption mode, integrity is always checked. Provided parameter {nameof(requestedIntegrityCheck)} cannot be false.");
            }
        }
    }
}
