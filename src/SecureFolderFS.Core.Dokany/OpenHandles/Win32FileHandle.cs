using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Shared.Utilities;
using System.IO;

#pragma warning disable CA1416 // Callsite is not supported on all platforms

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <inheritdoc cref="FileHandle"/>
    internal sealed class Win32FileHandle : FileHandle
    {
        public Win32FileHandle(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Sets file time for this file handle.
        /// </summary>
        /// <param name="creationTime">Creation time to set.</param>
        /// <param name="lastAccessTime">Last access time to set.</param>
        /// <param name="lastWriteTime">Last write time to set.</param>
        /// <returns>If the time was set successfully, returns true, otherwise false.</returns>
        public bool SetFileTime(ref long creationTime, ref long lastAccessTime, ref long lastWriteTime)
        {
            var hFile = GetHandle();
            if (hFile is null)
                return false;

            return UnsafeNativeApis.SetFileTime(hFile, ref creationTime, ref lastAccessTime, ref lastWriteTime);

            SafeFileHandle? GetHandle()
            {
                return Stream switch
                {
                    IWrapper<Stream> { Inner: FileStream fileStream } => fileStream.SafeFileHandle,
                    FileStream fileStream2 => fileStream2.SafeFileHandle,
                    _ => null
                };
            }
        }

        /// <inheritdoc cref="FileStream.Lock"/>
        public void Lock(long position, long length)
        {
            if (Stream is IWrapper<Stream> { Inner: FileStream fileStream })
                fileStream.Lock(position, length);
        }

        /// <inheritdoc cref="FileStream.Unlock"/>
        public void Unlock(long position, long length)
        {
            if (Stream is IWrapper<Stream> { Inner: FileStream fileStream })
                fileStream.Unlock(position, length);
        }
    }
}
