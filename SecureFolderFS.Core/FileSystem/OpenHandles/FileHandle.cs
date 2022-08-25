using SecureFolderFS.Core.Sdk.Streams;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    internal sealed class FileHandle : HandleObject
    {
        public ICleartextFileStream HandleStream { get; }

        public FileHandle(ICleartextFileStream cleartextFileStream)
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
