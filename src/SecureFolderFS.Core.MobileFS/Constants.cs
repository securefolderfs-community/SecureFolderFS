namespace SecureFolderFS.Core.MobileFS
{
    public static class Constants
    {
        public static class IOS
        {
            public static class FileSystem
            {
                public const string FS_ID = "IOS_FILE_PROVIDER";
                public const string FS_NAME = "iOS File Provider";
            }
        }

        public static class Android
        {
            public static class FileSystem
            {
                public const string FS_ID = "ANDROID_DOCUMENTS_PROVIDER";
                public const string FS_NAME = "Android SAF";
            }

            internal static class Saf
            {
                public const string FILE_SYSTEM_ROOT_ID = "root";
                public const string ROOT_DIRECTORY_ID = "directoryRoot";
                public const string ROOT_DIRECTORY_SECONDARY_ID = "shallowRoot";

                public const int IC_LOCK_LOCK = 0x0108002f; // ic_lock_lock
            }
        }
    }
}
