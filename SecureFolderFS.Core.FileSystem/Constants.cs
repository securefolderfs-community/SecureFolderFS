namespace SecureFolderFS.Core.FileSystem
{
    public static class Constants
    {
        public const string UNC_NAME = "securefolderfs";
        public const string DIRECTORY_ID_FILENAME = "dirid.iv";
        public const int DIRECTORY_ID_SIZE = 16;
        public const int FILE_EOF = 0;
        public const ulong INVALID_HANDLE = 0UL;
        public const bool OPT_IN_FOR_OPTIONAL_DEBUG_TRACING = true;

        public static class Caching
        {
            public const int RECOMMENDED_SIZE_CHUNK = 6;
            public const int RECOMMENDED_SIZE_DIRECTORYID = 1000;
            public const int RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES = 2000;
            public const int RECOMMENDED_SIZE_CLEARTEXT_FILENAMES = 2000;
        }
    }
}
