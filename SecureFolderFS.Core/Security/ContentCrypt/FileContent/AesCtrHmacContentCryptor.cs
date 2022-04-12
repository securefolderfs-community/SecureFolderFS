using System;
using System.Linq;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Chunks.Implementation;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    /// <summary>
    /// Provides encryption and decryption of payload using AES-CTR + HMAC-SHA256.
    /// </summary>
    internal sealed class AesCtrHmacContentCryptor : BaseContentCryptor<AesCtrHmacFileHeader, CleartextAesCtrHmacChunk, CiphertextAesCtrHmacChunk>
    {
        private readonly MasterKey _masterKey;

        public override int ChunkCleartextSize { get; } = CleartextAesCtrHmacChunk.CHUNK_CLEARTEXT_SIZE;

        public override int ChunkFullCiphertextSize { get; } = CiphertextAesCtrHmacChunk.CHUNK_FULL_CIPHERTEXT_SIZE;

        public AesCtrHmacContentCryptor(MasterKey masterKey, IKeyCryptor keyCryptor, IChunkFactory chunkFactory)
            : base(keyCryptor, chunkFactory)
        {
            this._masterKey = masterKey;
        }

        protected override ICiphertextChunk EncryptChunk(CleartextAesCtrHmacChunk cleartextChunk, long chunkNumber, AesCtrHmacFileHeader fileHeader)
        {
            // Chunk nonce
            var chunkNonce = new byte[CiphertextAesCtrHmacChunk.CHUNK_NONCE_SIZE];
            secureRandom.GetBytes(chunkNonce);

            // Payload
            var ciphertextPayload = keyCryptor.AesCtrCrypt.AesCtrEncrypt(cleartextChunk.ToArray(), fileHeader.ContentKey, chunkNonce);

            // Calculate MAC
            var chunkMac = CalculateChunkMac(
                fileHeaderNonce: fileHeader.Nonce,
                chunkNumber: chunkNumber,
                chunkNonce: chunkNonce,
                ciphertextPayload: ciphertextPayload);

            // Construct ciphertextChunkBuffer
            var ciphertextChunkBuffer = new byte[CiphertextAesCtrHmacChunk.CHUNK_NONCE_SIZE + ciphertextPayload.Length + CiphertextAesCtrHmacChunk.CHUNK_MAC_SIZE];
            ciphertextChunkBuffer.EmplaceArrays(chunkNonce, ciphertextPayload, chunkMac);

            return chunkFactory.FromCiphertextChunkBuffer(ciphertextChunkBuffer);
        }

        protected override ICleartextChunk DecryptChunk(CiphertextAesCtrHmacChunk ciphertextChunk, long chunkNumber, AesCtrHmacFileHeader fileHeader)
        {
            var cleartextChunkBuffer = keyCryptor.AesCtrCrypt.AesCtrDecrypt(ciphertextChunk.Payload, fileHeader.ContentKey, ciphertextChunk.Nonce);

            return chunkFactory.FromCleartextChunkBuffer(ExtendCleartextChunkBuffer(cleartextChunkBuffer), cleartextChunkBuffer.Length);
        }

        protected override void CheckIntegrity(CiphertextAesCtrHmacChunk ciphertextChunk,AesCtrHmacFileHeader fileHeader, long chunkNumber, bool requestedIntegrityCheck)
        {
            if (requestedIntegrityCheck && !CheckChunkMacIntegrity(fileHeader.Nonce, ciphertextChunk, chunkNumber))
            {
                throw UnauthenticChunkException.ForAesCtrHmac(); // TODO: Lower in code, where this is caught, report to HealthAPI
            }
        }

        private bool CheckChunkMacIntegrity(byte[] fileHeaderNonce, CiphertextAesCtrHmacChunk ciphertextAesCtrHmacChunk, long chunkNumber)
        {
            var realMac = CalculateChunkMac(
                fileHeaderNonce: fileHeaderNonce,
                chunkNumber: chunkNumber,
                chunkNonce: ciphertextAesCtrHmacChunk.Nonce,
                ciphertextPayload: ciphertextAesCtrHmacChunk.Payload);

            return realMac.SequenceEqual(ciphertextAesCtrHmacChunk.Auth);
        }

        private byte[] CalculateChunkMac(byte[] fileHeaderNonce, long chunkNumber, byte[] chunkNonce, byte[] ciphertextPayload)
        {
            using var macKey = _masterKey.CreateMacKeyCopy();
            var beChunkNumber = BitConverter.GetBytes(chunkNumber).AsBigEndian();

            using var hmacSha256Crypt = keyCryptor.HmacSha256Crypt.GetInstance(macKey);
            hmacSha256Crypt.InitializeHMAC();
            hmacSha256Crypt.Update(fileHeaderNonce);
            hmacSha256Crypt.Update(beChunkNumber);
            hmacSha256Crypt.Update(chunkNonce);
            hmacSha256Crypt.DoFinal(ciphertextPayload);

            return hmacSha256Crypt.GetHash();
        }

        public override void Dispose()
        {
            base.Dispose();
            _masterKey.Dispose();
        }
    }
}
