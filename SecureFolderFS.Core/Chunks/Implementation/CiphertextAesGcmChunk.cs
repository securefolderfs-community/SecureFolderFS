using System;

namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal sealed class CiphertextAesGcmChunk : BaseCiphertextChunk
    {
        public const int CHUNK_NONCE_SIZE = 12;

        public const int CHUNK_TAG_SIZE = 16;

        public const int CHUNK_FULL_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CleartextAesGcmChunk.CHUNK_CLEARTEXT_SIZE + CHUNK_TAG_SIZE;

        private CiphertextAesGcmChunk(byte[] nonce, byte[] payload, byte[] auth)
            : base(nonce, payload, auth)
        {
        }

        public static CiphertextAesGcmChunk FromCiphertextChunkBuffer(byte[] ciphertextChunkBuffer)
        {
            var nonce = new byte[CHUNK_NONCE_SIZE];
            var payload = new byte[ciphertextChunkBuffer.Length - (CHUNK_NONCE_SIZE + CHUNK_TAG_SIZE)];
            var tag = new byte[CHUNK_TAG_SIZE];

            Array.Copy(ciphertextChunkBuffer, 0, nonce, 0, CHUNK_NONCE_SIZE);
            Array.Copy(ciphertextChunkBuffer, CHUNK_NONCE_SIZE, payload, 0, payload.Length);
            Array.Copy(ciphertextChunkBuffer, CHUNK_NONCE_SIZE + payload.Length, tag, 0, CHUNK_TAG_SIZE);

            return new CiphertextAesGcmChunk(nonce, payload, tag);
        }
    }
}
