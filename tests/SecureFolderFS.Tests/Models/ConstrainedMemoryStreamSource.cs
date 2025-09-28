using SecureFolderFS.Storage.Streams;

namespace SecureFolderFS.Tests.Models
{
    public sealed class ConstrainedMemoryStreamSource : IStreamSource
    {
        /// <inheritdoc/>
        public MemoryStream GetInMemoryStream()
        {
            return new MemoryStream();
        }

        /// <inheritdoc/>
        public Stream WrapStreamSource(FileAccess access, Stream dataStream)
        {
            dataStream.Position = 0L;
            return new NonDisposableStream(dataStream, false, access.HasFlag(FileAccess.Read));
        }
    }
}