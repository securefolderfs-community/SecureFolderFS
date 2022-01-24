using System;

namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal sealed class CiphertextAesCtrHmacChunk : BaseCiphertextChunk
    {
        public const int CHUNK_NONCE_SIZE = 16;

        public const int CHUNK_MAC_SIZE = 32;

        public const int CHUNK_FULL_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CleartextAesCtrHmacChunk.CHUNK_CLEARTEXT_SIZE + CHUNK_MAC_SIZE;

        private CiphertextAesCtrHmacChunk(byte[] nonce, byte[] payload, byte[] auth)
            : base(nonce, payload, auth)
        {
        }

        public static CiphertextAesCtrHmacChunk FromCiphertextChunkBuffer(byte[] ciphertextChunkBuffer)
        {
            var nonce = new byte[CHUNK_NONCE_SIZE];
            var payload = new byte[ciphertextChunkBuffer.Length - (CHUNK_NONCE_SIZE + CHUNK_MAC_SIZE)];
            var mac = new byte[CHUNK_MAC_SIZE];

            Array.Copy(ciphertextChunkBuffer, 0, nonce, 0, CHUNK_NONCE_SIZE);
            Array.Copy(ciphertextChunkBuffer, CHUNK_NONCE_SIZE, payload, 0, payload.Length);
            Array.Copy(ciphertextChunkBuffer, CHUNK_NONCE_SIZE + payload.Length, mac, 0, CHUNK_MAC_SIZE);

            return new CiphertextAesCtrHmacChunk(nonce, payload, mac);
        }
    }
}
