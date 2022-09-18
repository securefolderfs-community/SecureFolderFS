using System.IO;
using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Core.Dokany.Streams;
using SecureFolderFS.Core.Dokany.UnsafeNative;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <summary>
    /// Represents a file handle on the virtual file system.
    /// </summary>
    internal sealed class FileHandle : ObjectHandle
    {
        /// <summary>
        /// Gets the stream of the file.
        /// </summary>
        public Stream FileStream { get; }

        public FileHandle(Stream fileStream)
        {
            FileStream = fileStream;
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
                if (FileStream is IStreamToFileInternal streamToFileInternal)
                    return streamToFileInternal.SafeFileHandle;

                if (FileStream is FileStream fileStreamImpl)
                    return fileStreamImpl.SafeFileHandle;

                return null;
            }
        }

        /// <inheritdoc cref="FileStream.Lock"/>
        public void Lock(long position, long length)
        {
            if (FileStream is IStreamToFileInternal streamToFileInternal)
            {
                streamToFileInternal.Lock(position, length);
            }
            else if (FileStream is FileStream fileStreamImpl)
            {
                fileStreamImpl.Lock(position, length);
            }
        }

        /// <inheritdoc cref="FileStream.Unlock"/>
        public void Unlock(long position, long length)
        {
            if (FileStream is IStreamToFileInternal streamToFileInternal)
            {
                streamToFileInternal.Unlock(position, length);
            }
            else if (FileStream is FileStream fileStreamImpl)
            {
                fileStreamImpl.Unlock(position, length);
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            FileStream.Dispose();
        }
    }
}
