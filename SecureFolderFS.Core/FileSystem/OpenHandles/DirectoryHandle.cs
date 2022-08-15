using System;
using System.IO;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.UnsafeNative;

using FileAccess = DokanNet.FileAccess;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class DirectoryHandle : HandleObject
    {
        private readonly IntPtr _hFolder;

        private DirectoryHandle(IntPtr hFolder)
        {
            _hFolder = hFolder;
        }

        public static DirectoryHandle Open(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
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

            return new DirectoryHandle(IntPtr.Zero);
        }

        public override void Dispose()
        {
            UnsafeNativeApis.CloseHandle(_hFolder);
        }
    }
}
