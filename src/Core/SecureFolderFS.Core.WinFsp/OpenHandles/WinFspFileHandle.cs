using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using System;
using System.IO;
using FileInfo = Fsp.Interop.FileInfo;

namespace SecureFolderFS.Core.WinFsp.OpenHandles
{
    internal sealed class WinFspFileHandle : FileHandle
    {
        private readonly Security _security;

        public System.IO.FileInfo FileInfo { get; }

        public WinFspFileHandle(System.IO.FileInfo fileInfo, Security security, Stream stream)
            : base(stream)
        {
            FileInfo = fileInfo;
            _security = security;
        }

        public FileInfo GetFileInfo()
        {
            var size = (ulong)_security.ContentCrypt.CalculatePlaintextSize(Math.Max(0L, FileInfo.Length - _security.HeaderCrypt.HeaderCiphertextSize));
            return new()
            {
                FileAttributes = (uint)FileInfo.Attributes,
                ReparseTag = 0,
                FileSize = size,
                AllocationSize = (size + Constants.WinFsp.ALLOCATION_UNIT - 1) / Constants.WinFsp.ALLOCATION_UNIT * Constants.WinFsp.ALLOCATION_UNIT,
                CreationTime = (ulong)FileInfo.CreationTimeUtc.ToFileTimeUtc(),
                LastAccessTime = (ulong)FileInfo.LastAccessTimeUtc.ToFileTimeUtc(),
                LastWriteTime = (ulong)FileInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                ChangeTime = (ulong)FileInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                IndexNumber = 0UL,
                HardLinks = 0u
            };
        }
    }
}
