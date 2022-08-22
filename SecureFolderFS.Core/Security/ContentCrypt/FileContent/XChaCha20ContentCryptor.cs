using System;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Chunks.Implementation;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Security.Cipher;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    internal sealed class XChaCha20ContentCryptor : BaseContentCryptor<XChaCha20FileHeader, CleartextXChaCha20Chunk, CiphertextXChaCha20Chunk>
    {
        public override int ChunkCleartextSize { get; } = CleartextXChaCha20Chunk.CHUNK_CLEARTEXT_SIZE;

        public override int ChunkFullCiphertextSize { get; } = CiphertextXChaCha20Chunk.CHUNK_FULL_CIPHERTEXT_SIZE;

        public XChaCha20ContentCryptor(ICipherProvider keyCryptor, IChunkFactory chunkFactory)
            : base(keyCryptor, chunkFactory)
        {
        }

        protected override ICiphertextChunk EncryptChunk(CleartextXChaCha20Chunk cleartextChunk, long chunkNumber, XChaCha20FileHeader fileHeader)
        {
            var fullCiphertextChunk = new byte[CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE + cleartextChunk.ActualLength + CiphertextXChaCha20Chunk.CHUNK_TAG_SIZE];
            var fullCiphertextChunkSpan = fullCiphertextChunk.AsSpan();

            // Chunk nonce
            secureRandom.GetBytes(fullCiphertextChunk.SliceArray(0, CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE));

            // Big Endian chunk number and file header nonce
            var beChunkNumberWithFileHeaderNonce = new byte[sizeof(long) + fileHeader.Nonce.Length];
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();

            Buffer.BlockCopy(beChunkNumber, 0, beChunkNumberWithFileHeaderNonce, 0, beChunkNumber.Length);
            Buffer.BlockCopy(fileHeader.Nonce, 0, beChunkNumberWithFileHeaderNonce, beChunkNumber.Length, fileHeader.Nonce.Length);

            // Payload
            keyCryptor.XChaCha20Poly1305Crypt.Encrypt(
                cleartextChunk.AsSpan(),
                fileHeader.ContentKey,
                fullCiphertextChunkSpan.Slice(0, CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE),
                fullCiphertextChunkSpan.Slice(CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE),
                beChunkNumberWithFileHeaderNonce);

            return chunkFactory.FromCiphertextChunkBuffer(fullCiphertextChunk);
        }

        protected override ICleartextChunk DecryptChunk(CiphertextXChaCha20Chunk ciphertextChunk, long chunkNumber, XChaCha20FileHeader fileHeader)
        {
            var fullCleartextBuffer = new byte[ChunkCleartextSize];
            var fullCleartextBufferSpan = fullCleartextBuffer.AsSpan();

            // Big Endian chunk number and file header nonce
            var beChunkNumberWithFileHeaderNonce = new byte[sizeof(long) + fileHeader.Nonce.Length];
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();

            Buffer.BlockCopy(beChunkNumber, 0, beChunkNumberWithFileHeaderNonce, 0, beChunkNumber.Length);
            Buffer.BlockCopy(fileHeader.Nonce, 0, beChunkNumberWithFileHeaderNonce, beChunkNumber.Length, fileHeader.Nonce.Length);

            // Decrypt
            var result = keyCryptor.XChaCha20Poly1305Crypt.Decrypt(
                ciphertextChunk.GetPayloadWithTagAsSpan(),
                fileHeader.ContentKey,
                ciphertextChunk.GetNonceAsSpan(),
                fullCleartextBufferSpan.Slice(0, ciphertextChunk.Buffer.Length - (CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE + CiphertextXChaCha20Chunk.CHUNK_TAG_SIZE)),
                beChunkNumberWithFileHeaderNonce);

            if (!result)
            {
                throw UnauthenticChunkException.ForXChaCha20();
            }

            return chunkFactory.FromCleartextChunkBuffer(fullCleartextBuffer, ciphertextChunk.Buffer.Length - (CiphertextXChaCha20Chunk.CHUNK_NONCE_SIZE + CiphertextXChaCha20Chunk.CHUNK_TAG_SIZE));
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
