using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Chunks.Implementation;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    /// <summary>
    /// Provides encryption and decryption of content using AES-GCM.
    /// </summary>
    internal sealed class AesGcmContentCryptor : BaseContentCryptor<AesGcmFileHeader, CleartextAesGcmChunk, CiphertextAesGcmChunk>
    {
        public override int ChunkCleartextSize { get; } = CleartextAesGcmChunk.CHUNK_CLEARTEXT_SIZE;

        public override int ChunkFullCiphertextSize { get; } = CiphertextAesGcmChunk.CHUNK_FULL_CIPHERTEXT_SIZE;

        public AesGcmContentCryptor(IKeyCryptor keyCryptor, IChunkFactory chunkFactory)
            : base(keyCryptor, chunkFactory)
        {
        }

        protected override ICiphertextChunk EncryptChunk(CleartextAesGcmChunk cleartextChunk, long chunkNumber, AesGcmFileHeader fileHeader)
        {
            // Chunk nonce
            var chunkNonce = new byte[CiphertextAesGcmChunk.CHUNK_NONCE_SIZE];
            secureRandom.GetBytes(chunkNonce);

            // Big Endian chunk number and file header nonce
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();
            var beChunkNumberWithFileHeaderNonce = new byte[beChunkNumber.Length + fileHeader.Nonce.Length];
            beChunkNumberWithFileHeaderNonce.EmplaceArrays(beChunkNumber, fileHeader.Nonce);

            // Payload
            var ciphertextPayload = keyCryptor.AesGcmCrypt.AesGcmEncrypt(
                cleartextChunk.ToArray(),
                fileHeader.ContentKey,
                chunkNonce,
                out var tag,
                beChunkNumberWithFileHeaderNonce);

            // Construct ciphertextChunkBuffer
            var ciphertextChunkBuffer = new byte[CiphertextAesGcmChunk.CHUNK_NONCE_SIZE + ciphertextPayload.Length + CiphertextAesGcmChunk.CHUNK_TAG_SIZE];
            ciphertextChunkBuffer.EmplaceArrays(chunkNonce, ciphertextPayload, tag);

            return chunkFactory.FromCiphertextChunkBuffer(ciphertextChunkBuffer);
        }

        protected override ICleartextChunk DecryptChunk(CiphertextAesGcmChunk ciphertextChunk, long chunkNumber, AesGcmFileHeader fileHeader)
        {
            try
            {
                // Big Endian chunk number and file header nonce
                var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();
                var beChunkNumberWithFileHeaderNonce = new byte[beChunkNumber.Length + fileHeader.Nonce.Length];
                beChunkNumberWithFileHeaderNonce.EmplaceArrays(beChunkNumber, fileHeader.Nonce);

                var cleartextChunkBuffer = keyCryptor.AesGcmCrypt.AesGcmDecrypt(
                    ciphertextChunk.Payload,
                    fileHeader.ContentKey,
                    ciphertextChunk.Nonce,
                    ciphertextChunk.Auth,
                    beChunkNumberWithFileHeaderNonce);

                return chunkFactory.FromCleartextChunkBuffer(ExtendCleartextChunkBuffer(cleartextChunkBuffer), cleartextChunkBuffer.Length);
            }
            catch (CryptographicException)
            {
                throw UnauthenticChunkException.ForAesGcm(); // TODO: Lower in code, where this is caught, report to HealthAPI
            }
        }

        protected override void CheckIntegrity(CiphertextAesGcmChunk ciphertextChunk, AesGcmFileHeader fileHeader, long chunkNumber, bool requestedIntegrityCheck)
        {
            if (!requestedIntegrityCheck)
            {
                throw new InvalidOperationException($"In AES-GCM encryption mode, integrity is always checked. Provided parameter {nameof(requestedIntegrityCheck)} cannot be false.");
            }
        }
    }
}
