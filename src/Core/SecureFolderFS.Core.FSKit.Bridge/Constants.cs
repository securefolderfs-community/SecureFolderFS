namespace SecureFolderFS.Core.FSKit.Bridge
{
    public static class Constants
    {
        public static class FileSystem
        {
            public const string FS_ID = "FSKit";
            public const string FS_NAME = "MacOS FSKit";
        }

        public static class IPC
        {
            public const string COMMAND_MOUNT = "mount";
            public const string COMMAND_UNMOUNT = "unmount";
            public const string COMMAND_GET_STATUS = "get_status";
            public const string RESPONSE_SUCCESS = "success";
            public const string RESPONSE_ERROR = "error";
        }
    }
}