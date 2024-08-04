namespace SecureFolderFS.Core.MobileFS
{
    public static class Constants
    {
        public const string ANDROID_FILE_SYSTEM_NAME = "Android SAF";

        internal static class AndroidSaf
        {
            public const string FILE_SYSTEM_ROOT_ID = "root";
            public const string ROOT_DIRECTORY_ID = "directoryRoot";
            public const string ROOT_DIRECTORY_SECONDARY_ID = "shallowRoot";

            public const int IC_LOCK_LOCK = 0x0108002f; // ic_lock_lock
        }
    }
}
