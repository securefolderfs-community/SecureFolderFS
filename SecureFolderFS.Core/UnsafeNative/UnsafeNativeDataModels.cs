using System;
using System.IO;

namespace SecureFolderFS.Core.UnsafeNative
{
    internal static class UnsafeNativeDataModels
    {
        public static class FILE_ACCESS
        {
            public const uint GENERIC_READ = 0x80000000u;
            public const uint GENERIC_WRITE = 0x40000000u;
            public const uint GENERIC_ALL = 0x10000000u;

            public static uint FromFileAccess(FileAccess access)
            {
                return access switch
                {
                    FileAccess.Read => GENERIC_READ,
                    FileAccess.Write => GENERIC_WRITE,
                    FileAccess.ReadWrite => GENERIC_ALL,
                    _ => throw new ArgumentOutOfRangeException(nameof(access))
                };
            }
        }

        public static class FILE_SHARE
        {
            public const uint FILE_SHARE_READ = 0x00000001u;
            public const uint FILE_SHARE_WRITE = 0x00000002u;
            public const uint FILE_SHARE_DELETE = 0x00000004u;

            public static uint FromFileShare(FileShare share)
            {
                return (uint)share;
            }
        }

        public static class FILE_MODE
        {
            public const uint CREATE_ALWAYS = 2u;
            public const uint CREATE_NEW = 1u;
            public const uint OPEN_ALWAYS = 4u;
            public const uint OPEN_EXISTING = 3u;
            public const uint TRUNCATE_EXISTING = 5u;

            public static uint FromFileMode(FileMode mode)
            {
                return (uint)mode;
            }
        }

        public static class FILE_OPTIONS
        {
            public const uint FILE_ATTRIBUTE_READONLY = 0x1u;
            public const uint FILE_ATTRIBUTE_HIDDEN = 0x2u;
            public const uint FILE_ATTRIBUTE_SYSTEM = 0x4u;
            public const uint FILE_ATTRIBUTE_DIRECTORY = 0x10u;
            public const uint FILE_ATTRIBUTE_ARCHIVE = 0x20u;
            public const uint FILE_ATTRIBUTE_DEVICE = 0x40u;
            public const uint FILE_ATTRIBUTE_NORMAL = 0x80u;
            public const uint FILE_ATTRIBUTE_TEMPORARY = 0x100u;
            public const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x200u;
            public const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x400u;
            public const uint FILE_ATTRIBUTE_COMPRESSED = 0x800u;
            public const uint FILE_ATTRIBUTE_OFFLINE = 0x1000u;
            public const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x2000u;
            public const uint FILE_ATTRIBUTE_ENCRYPTED = 0x4000u;
            public const uint FILE_ATTRIBUTE_VIRTUAL = 0x10000u;

            public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000u;
            public const uint FILE_FLAG_DELETE_ON_CLOSE = 0x4000000u;
            public const uint FILE_FLAG_NO_BUFFERING = 0x20000000u;
            public const uint FILE_FLAG_OPEN_NO_RECALL = 0x100000u;
            public const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x200000u;
            public const uint FILE_FLAG_OVERLAPPED = 0x40000000u;
            public const uint FILE_FLAG_POSIX_SEMANTICS = 0x1000000u;
            public const uint FILE_FLAG_RANDOM_ACCESS = 0x10000000u;
            public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x8000000u;
            public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000u;

            public static uint FromFileOptions(FileOptions options)
            {
                return options switch
                {
                    FileOptions.WriteThrough => FILE_FLAG_WRITE_THROUGH,
                    FileOptions.Encrypted => FILE_ATTRIBUTE_ENCRYPTED,
                    FileOptions.DeleteOnClose => FILE_FLAG_DELETE_ON_CLOSE,
                    FileOptions.SequentialScan => FILE_FLAG_SEQUENTIAL_SCAN,
                    FileOptions.RandomAccess => FILE_FLAG_RANDOM_ACCESS,
                    FileOptions.Asynchronous => FILE_FLAG_OVERLAPPED,
                    FileOptions.None => 0x00000000u,
                    _ => throw new ArgumentOutOfRangeException(nameof(options))
                };
            }
        }
    }
}
