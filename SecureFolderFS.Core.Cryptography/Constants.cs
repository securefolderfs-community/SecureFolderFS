namespace SecureFolderFS.Core.Cryptography
{
    public static class Constants
    {
        internal static class Security
        {
            internal static class Chunks
            {
                internal static class XChaCha20Poly1305
                {
                    public const int CHUNK_CLEARTEXT_SIZE = 32 * 1024; // 32768
                    public const int CHUNK_NONCE_SIZE = 24;
                    public const int CHUNK_TAG_SIZE = 16;
                    public const int CHUNK_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CHUNK_CLEARTEXT_SIZE + CHUNK_TAG_SIZE;
                }

                internal static class AesGcm
                {
                    public const int CHUNK_CLEARTEXT_SIZE = 32 * 1024; // 32768
                    public const int CHUNK_NONCE_SIZE = 12;
                    public const int CHUNK_TAG_SIZE = 16;
                    public const int CHUNK_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CHUNK_CLEARTEXT_SIZE + CHUNK_TAG_SIZE;
                }

                internal static class AesCtrHmac
                {
                    public const int CHUNK_CLEARTEXT_SIZE = 32 * 1024; // 32768
                    public const int CHUNK_NONCE_SIZE = 16;
                    public const int CHUNK_MAC_SIZE = 32;
                    public const int CHUNK_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CHUNK_CLEARTEXT_SIZE + CHUNK_MAC_SIZE;
                }
            }

            internal static class Headers
            {
                internal static class XChaCha20Poly1305
                {
                    public const int HEADER_NONCE_SIZE = 24;
                    public const int HEADER_CONTENTKEY_SIZE = 32;
                    public const int HEADER_TAG_SIZE = 16;
                    public const int HEADER_SIZE = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE + HEADER_TAG_SIZE;
                }

                internal static class AesGcm
                {
                    public const int HEADER_NONCE_SIZE = 12;
                    public const int HEADER_CONTENTKEY_SIZE = 32;
                    public const int HEADER_TAG_SIZE = 16;
                    public const int HEADER_SIZE = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE + HEADER_TAG_SIZE;
                }

                internal static class AesCtrHmac
                {
                    public const int HEADER_NONCE_SIZE = 16;
                    public const int HEADER_CONTENTKEY_SIZE = 32;
                    public const int HEADER_MAC_SIZE = 32;
                    public const int HEADER_SIZE = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE + HEADER_MAC_SIZE;
                }
            }

            internal static class KeyChains
            {
                public const int ENCRYPTION_KEY_LENGTH = 32;
                public const int MAC_KEY_LENGTH = 32;
                public const int SALT_LENGTH = 16;
            }

            internal static class CryptImpl
            {
                internal static class Argon2
                {
                    public const int DEGREE_OF_PARALLELISM = 8;
                    public const int ITERATIONS = 8;
                    public const int MEMORY_SIZE = 102400;
                }
            }
        }
    }
}
