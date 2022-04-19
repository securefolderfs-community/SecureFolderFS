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
            var fullCiphertextChunk = new byte[CiphertextAesGcmChunk.CHUNK_NONCE_SIZE + cleartextChunk.ActualLength + CiphertextAesGcmChunk.CHUNK_TAG_SIZE];
            var fullCiphertextChunkSpan = fullCiphertextChunk.AsSpan();

            // Chunk nonce
            secureRandom.GetBytes(fullCiphertextChunkSpan.Slice(0, CiphertextAesGcmChunk.CHUNK_NONCE_SIZE));

            // Big Endian chunk number and file header nonce
            var beChunkNumberWithFileHeaderNonce = new byte[sizeof(long) + fileHeader.Nonce.Length];
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();

            Buffer.BlockCopy(beChunkNumber, 0, beChunkNumberWithFileHeaderNonce, 0, beChunkNumber.Length);
            Buffer.BlockCopy(fileHeader.Nonce, 0, beChunkNumberWithFileHeaderNonce, beChunkNumber.Length, fileHeader.Nonce.Length);

            // Payload
            keyCryptor.AesGcmCrypt.AesGcmEncrypt2(
                cleartextChunk.AsSpan(),
                fileHeader.ContentKey,
                fullCiphertextChunkSpan.Slice(0, CiphertextAesGcmChunk.CHUNK_NONCE_SIZE),
                fullCiphertextChunkSpan.Slice(fullCiphertextChunkSpan.Length - CiphertextAesGcmChunk.CHUNK_TAG_SIZE),
                fullCiphertextChunkSpan.Slice(CiphertextAesGcmChunk.CHUNK_NONCE_SIZE, cleartextChunk.ActualLength),
                beChunkNumberWithFileHeaderNonce);

            return chunkFactory.FromCiphertextChunkBuffer(fullCiphertextChunk);
        }

        protected override ICleartextChunk DecryptChunk(CiphertextAesGcmChunk ciphertextChunk, long chunkNumber, AesGcmFileHeader fileHeader)
        {
            try
            {
                var fullCleartextBuffer = new byte[ChunkCleartextSize];
                var fullCleartextBufferSpan = fullCleartextBuffer.AsSpan();

                // Big Endian chunk number and file header nonce
                var beChunkNumberWithFileHeaderNonce = new byte[sizeof(long) + fileHeader.Nonce.Length];
                var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();

                Buffer.BlockCopy(beChunkNumber, 0, beChunkNumberWithFileHeaderNonce, 0, beChunkNumber.Length);
                Buffer.BlockCopy(fileHeader.Nonce, 0, beChunkNumberWithFileHeaderNonce, beChunkNumber.Length, fileHeader.Nonce.Length);

                // Decrypt
                keyCryptor.AesGcmCrypt.AesGcmDecrypt2(
                    ciphertextChunk.GetPayloadAsSpan(),
                    fileHeader.ContentKey,
                    ciphertextChunk.GetNonceAsSpan(),
                    ciphertextChunk.GetAuthAsSpan(),
                    fullCleartextBufferSpan.Slice(0, ciphertextChunk.Buffer.Length - (CiphertextAesGcmChunk.CHUNK_NONCE_SIZE + CiphertextAesGcmChunk.CHUNK_TAG_SIZE)),
                    beChunkNumberWithFileHeaderNonce);

                return chunkFactory.FromCleartextChunkBuffer(fullCleartextBuffer, ciphertextChunk.Buffer.Length - (CiphertextAesGcmChunk.CHUNK_NONCE_SIZE + CiphertextAesGcmChunk.CHUNK_TAG_SIZE));
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
