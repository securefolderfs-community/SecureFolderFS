using System.IO;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class FileHandle : HandleObject
    {
        public Stream HandleStream { get; }

        public FileHandle(Stream cleartextFileStream)
        {
            HandleStream = cleartextFileStream;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            HandleStream.Dispose();
        }
    }
}
