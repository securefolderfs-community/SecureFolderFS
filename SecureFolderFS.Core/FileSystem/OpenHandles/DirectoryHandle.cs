using System;
using System.IO;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Storage;
using SecureFolderFS.Core.UnsafeNative;
using SecureFolderFS.Core.Helpers;

using FileAccess = DokanNet.FileAccess;
using static SecureFolderFS.Core.UnsafeNative.UnsafeNativeDataModels;
using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class DirectoryHandle : HandleObject
    {
        private readonly IntPtr _hFolder;

        public IVaultFolder VaultFolder { get; }

        private DirectoryHandle(IVaultFolder vaultFolder, IntPtr hFolder)
            : base(vaultFolder.CiphertextPath)
        {
            VaultFolder = vaultFolder;
            _hFolder = hFolder;
        }

        public static DirectoryHandle Open(ICiphertextPath ciphertextPath, IVaultStorageReceiver vaultStorageReceiver, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            _ = mode;

            //IntPtr hFolder = UnsafeNativeApis.CreateFile(
            //    PathHelpers.EnsureNoTrailingPathSeparator(ciphertextPath.Path),
            //    (uint)access,
            //    FILE_SHARE.FromFileShare(share),
            //    IntPtr.Zero,
            //    FILE_MODE.OPEN_EXISTING,
            //    FILE_OPTIONS.FromFileOptions(options) | FILE_OPTIONS.FILE_FLAG_BACKUP_SEMANTICS,
            //    IntPtr.Zero);

            //if (hFolder.ToInt64() == -1L)
            //{
            //    int hresult = Marshal.GetHRForLastWin32Error();
            //    int err = Marshal.GetLastWin32Error();
            //    //throw new IOException("Couldn't open hFolder.", hresult);
            //}
            //else
            //{

            //}

            var vaultFolder = vaultStorageReceiver.OpenVaultFolder(ciphertextPath);
            return new DirectoryHandle(vaultFolder, IntPtr.Zero);
        }

        public override void Dispose()
        {
            UnsafeNativeApis.CloseHandle(_hFolder);
        }
    }
}
