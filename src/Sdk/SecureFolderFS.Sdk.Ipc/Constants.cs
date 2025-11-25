namespace SecureFolderFS.Sdk.Ipc
{
    public static class Constants
    {
        public static class Commands
        {
            public const string PING = "ping";
            public const string GET_STATUS = "get_status";

            public const string MOUNT = "mount";
            public const string UNMOUNT = "unmount";
        }

        public static class Responses
        {
            public const string SUCCESS = "success";
            public const string ERROR = "error";
        }

        public static class Sockets
        {
            public const string SOCKET_DIRECTORY_NAME = ".securefolder";
            public const string SOCKET_FILENAME = "fskit.sock";
        }
    }
}