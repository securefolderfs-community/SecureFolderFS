using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    /// <summary>
    /// Represents a file handle on the virtual file system.
    /// </summary>
    public abstract class FileHandle : IDisposable
    {
        /// <summary>
        /// Gets the stream of the file.
        /// </summary>
        public Stream Stream { get; }

        protected FileHandle(Stream stream)
        {
            Stream = stream;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Stream.Dispose();
        }
    }
}
