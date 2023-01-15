using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Streams;
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
                if (Stream is CleartextStream { BaseStream: FileStream fileStream })
                    return fileStream.SafeFileHandle;

                if (Stream is FileStream fileStream2)
                    return fileStream2.SafeFileHandle;

                return null;
            }
        }

        /// <inheritdoc cref="FileStream.Lock"/>
        public void Lock(long position, long length)
        {
            if (Stream is CleartextStream { BaseStream: FileStream fileStream })
            {
                fileStream.Lock(position, length);
            }
            else if (Stream is FileStream fileStream2)
            {
                fileStream2.Lock(position, length);
            }
        }

        /// <inheritdoc cref="FileStream.Unlock"/>
        public void Unlock(long position, long length)
        {
            if (Stream is CleartextStream { BaseStream: FileStream fileStream })
            {
                fileStream.Unlock(position, length);
            }
            else if (Stream is FileStream fileStream2)
            {
                fileStream2.Unlock(position, length);
            }
        }
    }
}
