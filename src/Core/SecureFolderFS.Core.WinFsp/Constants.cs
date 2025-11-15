namespace SecureFolderFS.Core.WinFsp
{
    public static class Constants
    {
        public static class FileSystem
        {
            public const string FS_ID = "WINFSP";
            public const string FS_NAME = "WinFsp";
            public const string VERSION_STRING = "2.1.x";
        }

        internal static class WinFsp
        {
            public const int ALLOCATION_UNIT = 4096;
            public const ushort SECTOR_SIZE = 4096;
            public const ushort SECTORS_PER_UNIT = 1;
            public const ushort MAX_COMPONENT_LENGTH = 255;
            public const uint FILE_INFO_TIMEOUT = 1000u;
            public const string FS_TYPE_ID = "WinFsp";
            public const string SERVICE_NAME = "SecureFolderFS_WinFsp";

            public const ushort WINFSP_MIN_VERSION = 2023;
            public const ushort WINFSP_MAX_VERSION = 2025;
        }

        internal static class UnsafeNative
        {
            public const uint INVALID_FILE_ATTRIBUTES = 4294967295u;
        }
    }
}
