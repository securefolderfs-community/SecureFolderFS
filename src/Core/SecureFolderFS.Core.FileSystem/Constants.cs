namespace SecureFolderFS.Core.FileSystem
{
    public static class Constants
    {
        public const string LOCAL_FILE_SYSTEM_NAME = "AbstractStorage";
        public const string UNC_NAME = "securefolderfs";
        public const int FILE_EOF = 0;
        public const int DIRECTORY_ID_SIZE = 16;

        /// <summary>
        /// The time window within which previously recycled items are folded back into a
        /// recycled parent folder. OS clients (Finder, Explorer, WebDav/FUSE drivers) delete
        /// folder trees member-by-member; when the parent folder finally arrives at the recycle
        /// bin, children recycled within this window are reattached to it so the tree appears
        /// as a single restorable entry. Membership is proven by Directory ID, so this window
        /// only limits how far back unrelated same-folder deletions are pulled in.
        /// </summary>
        public const long RECYCLE_BIN_FOLD_WINDOW_MS = 60L * 60L * 1000L;
        public const ulong INVALID_HANDLE = 0UL;
        public const bool OPT_IN_FOR_OPTIONAL_DEBUG_TRACING = false;

        public static class FileSystem
        {
            public const string FS_ID = "ABSTRACT_STORAGE";
            public const string FS_NAME = "Local File System";
        }

        public static class Names
        {
            public const string ENCRYPTED_FILE_EXTENSION = ".sffs";
            public const string SHORTENED_FILE_EXTENSION = ".sffsn";
            public const string SIDECAR_FILE_EXTENSION = ".sffsi";
            public const string DIRECTORY_ID_FILENAME = "dirid.iv";
            public const string RECYCLE_BIN_NAME = "recycle_bin";
            public const string RECYCLE_BIN_CONFIGURATION_FILENAME = "recycle_bin.cfg";
        }

        public static class Caching
        {
            public const int RECOMMENDED_SIZE_CHUNKS = 6;
            public const int RECOMMENDED_SIZE_DIRECTORY_ID = 1000;
            public const int RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES = 2000;
            public const int RECOMMENDED_SIZE_PLAINTEXT_FILENAMES = 2000;
        }
    }
}
