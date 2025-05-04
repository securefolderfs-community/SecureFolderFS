namespace SecureFolderFS.Core.Cryptography
{
    public static class Constants
    {
        public static class KeyTraits
        {
            public const string KEY_TEXT_SEPARATOR = "@@@";
            public const int SALT_LENGTH = 16;
            public const int ENCKEY_LENGTH = 32;
            public const int MACKEY_LENGTH = 32;
            public const int ARGON2_KEK_LENGTH = 32;
            public const int CHALLENGE_KEY_PART_LENGTH = 128;
        }

        public static class CipherId
        {
            public const string NONE = ""; // Empty string
            public const string AES_SIV = "AES-SIV";
            public const string AES_GCM = "AES-GCM";
            public const string AES_CTR_HMAC = "AES-CTR HMAC";
            public const string XCHACHA20_POLY1305 = "XChaCha20-Poly1305";

            public const string ENCODING_BASE4K = "Base4K";
            public const string ENCODING_BASE64URL = "Base64Url";
        }

        internal static class Crypto
        {
            internal static class Chunks
            {
                internal static class XChaCha20Poly1305
                {
                    public const int CHUNK_PLAINTEXT_SIZE = 32 * 1024; // 32768
                    public const int CHUNK_NONCE_SIZE = 24;
                    public const int CHUNK_TAG_SIZE = 16;
                    public const int CHUNK_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CHUNK_PLAINTEXT_SIZE + CHUNK_TAG_SIZE;
                }

                internal static class AesGcm
                {
                    public const int CHUNK_PLAINTEXT_SIZE = 32 * 1024; // 32768
                    public const int CHUNK_NONCE_SIZE = 12;
                    public const int CHUNK_TAG_SIZE = 16;
                    public const int CHUNK_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CHUNK_PLAINTEXT_SIZE + CHUNK_TAG_SIZE;
                }

                internal static class AesCtrHmac
                {
                    public const int CHUNK_PLAINTEXT_SIZE = 32 * 1024; // 32768
                    public const int CHUNK_NONCE_SIZE = 16;
                    public const int CHUNK_MAC_SIZE = 32;
                    public const int CHUNK_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CHUNK_PLAINTEXT_SIZE + CHUNK_MAC_SIZE;
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

            internal static class Argon2
            {
                public const int MEMORY_SIZE_KIBIBYTES = 32768; // 32MiB
                public const int DEGREE_OF_PARALLELISM = 2;
                public const int ITERATIONS = 2;
            }
        }
    }
}
