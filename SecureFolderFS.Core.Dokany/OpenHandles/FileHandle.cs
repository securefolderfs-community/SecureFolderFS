using System.IO;

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

        /// <inheritdoc/>
        public override void Dispose()
        {
            FileStream.Dispose();
        }
    }
}
