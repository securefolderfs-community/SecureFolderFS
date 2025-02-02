﻿namespace SecureFolderFS.Core.FileSystem
{
    public static class Constants
    {
        public const string LOCAL_FILE_SYSTEM_NAME = "AbstractStorage";

        public const string UNC_NAME = "securefolderfs";
        public const int FILE_EOF = 0;
        public const int DIRECTORY_ID_SIZE = 16;
        public const ulong INVALID_HANDLE = 0UL;
        public const bool OPT_IN_FOR_OPTIONAL_DEBUG_TRACING = true;

        public static class FileSystem
        {
            public const string FS_ID = "ABSTRACT_STORAGE";
            public const string FS_NAME = "Local File System";
        }

        public static class Names
        {
            public const string ENCRYPTED_FILE_EXTENSION = ".sffs";
            public const string DIRECTORY_ID_FILENAME = "dirid.iv";
            public const string RECYCLE_BIN_NAME = "recycle_bin.vi";
        }

        public static class Caching
        {
            public const int RECOMMENDED_SIZE_CHUNK = 6;
            public const int RECOMMENDED_SIZE_DIRECTORY_ID = 1000;
            public const int RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES = 2000;
            public const int RECOMMENDED_SIZE_Plaintext_FILENAMES = 2000;
        }
    }
}
