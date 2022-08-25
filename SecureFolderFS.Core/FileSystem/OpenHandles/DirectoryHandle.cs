using SecureFolderFS.Core.UnsafeNative;
using System;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class DirectoryHandle : HandleObject
    {
        private readonly IntPtr _hFolder;

        public DirectoryHandle(IntPtr hFolder)
        {
            _hFolder = hFolder;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            UnsafeNativeApis.CloseHandle(_hFolder);
        }
    }
}
