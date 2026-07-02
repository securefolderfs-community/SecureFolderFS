using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Shared.Helpers;
using System;
using System.IO;
using FileInfo = Fsp.Interop.FileInfo;

namespace SecureFolderFS.Core.WinFsp.OpenHandles
{
    /// <inheritdoc cref="FileHandle"/>
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
            // System.IO.FileInfo caches its state on first access so refresh is needed to avoid returning
            // stale metadata after the file was modified through this (or another) handle
            FileInfo.Refresh();

            // The plaintext stream tracks the up-to-date length including data that
            // has not been flushed to the ciphertext file yet
            var plaintextSize = SafetyHelpers.NoFailureResult<long?>(() => Stream.Length);
            return ToFileInfo(FileInfo, _security, plaintextSize);
        }

        public static FileInfo ToFileInfo(System.IO.FileInfo fileInfo, Security security, long? plaintextSize = null)
        {
            var size = (ulong)(plaintextSize ?? security.ContentCrypt.CalculatePlaintextSize(Math.Max(0L, fileInfo.Length - security.HeaderCrypt.HeaderCiphertextSize)));
            return new()
            {
                FileAttributes = (uint)(fileInfo.Attributes == 0 ? FileAttributes.Normal : fileInfo.Attributes),
                ReparseTag = 0,
                FileSize = size,
                AllocationSize = (size + Constants.WinFsp.ALLOCATION_UNIT - 1) / Constants.WinFsp.ALLOCATION_UNIT * Constants.WinFsp.ALLOCATION_UNIT,
                CreationTime = (ulong)fileInfo.CreationTimeUtc.ToFileTimeUtc(),
                LastAccessTime = (ulong)fileInfo.LastAccessTimeUtc.ToFileTimeUtc(),
                LastWriteTime = (ulong)fileInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                ChangeTime = (ulong)fileInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                IndexNumber = 0UL,
                HardLinks = 0u
            };
        }
    }
}
