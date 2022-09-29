using System.IO;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <summary>
    /// Represents a file handle on the virtual file system.
    /// </summary>
    internal abstract class FileHandle : ObjectHandle
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
        public override void Dispose()
        {
            Stream.Dispose();
        }
    }
}
