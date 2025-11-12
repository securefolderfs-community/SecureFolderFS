using SecureFolderFS.Core.FileSystem.OpenHandles;
using System.IO;
using FileInfo = Fsp.Interop.FileInfo;

namespace SecureFolderFS.Core.WinFsp.OpenHandles
{
    internal sealed class WinFspDirectoryHandle : DirectoryHandle
    {
        public DirectoryInfo DirectoryInfo { get; }

        public WinFspDirectoryHandle(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
        }

        public FileInfo GetFileInfo()
        {
            return new()
            {
                FileAttributes = (uint)DirectoryInfo.Attributes,
                ReparseTag = 0,
                FileSize = 0UL,
                AllocationSize = (0UL + Constants.WinFsp.ALLOCATION_UNIT - 1) / Constants.WinFsp.ALLOCATION_UNIT * Constants.WinFsp.ALLOCATION_UNIT,
                CreationTime = (ulong)DirectoryInfo.CreationTimeUtc.ToFileTimeUtc(),
                LastAccessTime = (ulong)DirectoryInfo.LastAccessTimeUtc.ToFileTimeUtc(),
                LastWriteTime = (ulong)DirectoryInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                ChangeTime = (ulong)DirectoryInfo.LastWriteTimeUtc.ToFileTimeUtc(),
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
