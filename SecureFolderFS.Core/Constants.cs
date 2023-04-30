namespace SecureFolderFS.Core
{
    public static class Constants
    {
        public const string CONTENT_FOLDERNAME = "content";
        public const string VAULT_CONFIGURATION_FILENAME = "sfconfig.cfg";
        public const string VAULT_KEYSTORE_FILENAME = "keystore.cfg";
        public const string ENCRYPTED_FILE_EXTENSION = ".sffs";

        public static class FileSystemId
        {
            public const string DOKAN_ID = "DOKANY";
            public const string FUSE_ID = "FUSE";
            public const string WEBDAV_ID = "WEBDAV";
        }

        public static class CipherId
        {
            public const string NONE = "None";
            public const string AES_CTR_HMAC = "AES-CTR HMAC";
            public const string AES_GCM = "AES-GCM";
            public const string XCHACHA20_POLY1305 = "XChaCha20-Poly1305";
            public const string AES_SIV = "AES-SIV";
        }

        internal static class Caching
        {
            public const int CHUNK_CACHE_SIZE = 6;
            public const int DIRECTORY_ID_CACHE_SIZE = 1000;
            public const int CIPHERTEXT_FILENAMES_CACHE_SIZE = 2000;
            public const int CLEARTEXT_FILENAMES_CACHE_SIZE = 2000;
        }

        public static class KeyChains
        {
            public const int ENCKEY_LENGTH = 32;
            public const int MACKEY_LENGTH = 32;
            public const int SALT_LENGTH = 16;
        }

        public static class VaultVersion
        {
            public const int V1 = 1;
            public const int LATEST_VERSION = V1;
        }
    }
}
