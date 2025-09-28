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
            var canSeek = !access.HasFlag(FileAccess.Write);
            canSeek = false;
            return new NonDisposableStream(dataStream, canSeek, access.HasFlag(FileAccess.Read));
        }
    }
}