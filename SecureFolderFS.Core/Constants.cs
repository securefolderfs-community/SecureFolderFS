namespace SecureFolderFS.Core
{
    public static class Constants
    {
        public const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public const string CONTENT_FOLDERNAME = "content";

        public const string VAULT_CONFIGURATION_FILENAME = "sfconfig.cfg";

        public const string VAULT_KEYSTORE_FILENAME = "keystore.cfg";

        public const string DIRECTORY_ID_FILENAME = "dirid.iv";

        public const string ENCRYPTED_FILE_EXTENSION = ".sffs";

        internal static class FileSystem
        {
            public const int INVALID_FILE_ATTRIBUTES = -0x1;

            public const long INVALID_HANDLE = 0L;

            public const uint FILESYSTEM_SERIAL_NUMBER = 1137196800u;

            public const string FILESYSTEM_NAME = "SFFS";

            public const string UNC_NAME = "securefolderfs";

            public const int MAX_DRIVE_INFO_CALLS_UNTIL_GIVEUP = 10;

            internal static class Dokan
            {
                public const int DOKAN_VERSION = 150;

                public const int DOKAN_MAX_VERSION = 200;

                public const uint MAX_COMPONENT_LENGTH = 256;

                public const int THREAD_COUNT = 5; // TODO: Too low?

                public const int ALLOC_UNIT_SIZE = 512;

                public const int SECTOR_SIZE = 512;

                public const DokanNet.FileAccess DATA_ACCESS =
                                                          DokanNet.FileAccess.ReadData
                                                        | DokanNet.FileAccess.WriteData
                                                        | DokanNet.FileAccess.AppendData
                                                        | DokanNet.FileAccess.Execute
                                                        | DokanNet.FileAccess.GenericExecute
                                                        | DokanNet.FileAccess.GenericWrite
                                                        | DokanNet.FileAccess.GenericRead;

                public const DokanNet.FileAccess DATA_WRITE_ACCESS =
                                                             DokanNet.FileAccess.WriteData
                                                           | DokanNet.FileAccess.AppendData
                                                           | DokanNet.FileAccess.Delete
                                                           | DokanNet.FileAccess.GenericWrite;
            }
        }

        internal static class IO
        {
            public const int DIRECTORY_ID_MAX_SIZE = 16;

            public const int FILE_EOF = 0;

            public const int READ_BUFFER_SIZE = 4096;

            public const int WRITE_BUFFER_SIZE = 4096;

            public const int MAX_CACHED_CHUNKS = 6;

            public const int MAX_CACHED_DIRECTORY_IDS = 1000;

            public const int MAX_CACHED_CIPHERTEXT_FILENAMES = 2000;

            public const int MAX_CACHED_CLEARTEXT_FILENAMES = 2000;
        }

        internal static class Security
        {
            public const bool ALWAYS_CHECK_CHUNK_INTEGRITY = true;

            public static class KeyChains
            {
                public const int ENCRYPTIONKEY_LENGTH = 32;

                public const int MACKEY_LENGTH = 32;

                public const int SALT_LENGTH = 16;
            }

            public static class EncryptionAlgorithm
            {
                internal static class Argon2id
                {
                    public const int DEGREE_OF_PARALLELISM = 8;

                    public const int ITERATIONS = 8;

                    public const int MEMORY_SIZE = 102400; // 102400 Kb
                }

                public static class AesGcm
                {
                    public const int AES_GCM_TAG_SIZE = 16;
                }

                public static class XChaCha20
                {
                    public const int XCHACHA20_POLY1305_TAG_SIZE = 16;
                }
            }
        }
    }
}
