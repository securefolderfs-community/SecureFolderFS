using SecureFolderFS.Core.FileSystem.OpenHandles;
using System.IO;
using FileInfo = Fsp.Interop.FileInfo;

namespace SecureFolderFS.Core.WinFsp.OpenHandles
{
    /// <inheritdoc cref="DirectoryHandle"/>
    internal sealed class WinFspDirectoryHandle : DirectoryHandle
    {
        public DirectoryInfo DirectoryInfo { get; }

        public WinFspDirectoryHandle(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
        }

        public FileInfo GetFileInfo()
        {
            return ToFileInfo(DirectoryInfo);
        }

        public static FileInfo ToFileInfo(DirectoryInfo directoryInfo)
        {
            return new()
            {
                FileAttributes = (uint)directoryInfo.Attributes,
                ReparseTag = 0,
                FileSize = 0UL,
                AllocationSize = (0UL + Constants.WinFsp.ALLOCATION_UNIT - 1) / Constants.WinFsp.ALLOCATION_UNIT * Constants.WinFsp.ALLOCATION_UNIT,
                CreationTime = (ulong)directoryInfo.CreationTimeUtc.ToFileTimeUtc(),
                LastAccessTime = (ulong)directoryInfo.LastAccessTimeUtc.ToFileTimeUtc(),
                LastWriteTime = (ulong)directoryInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                ChangeTime = (ulong)directoryInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                IndexNumber = 0UL,
                HardLinks = 0u
            };
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
        }
    }
}
