using SecureFolderFS.Storage.MemoryStorageEx;

namespace SecureFolderFS.Tests.Models
{
    public sealed class ConstrainedMemoryStreamSource : IStreamSource
    {
        public bool CanSeek { get; }

        public bool CanRead { get; }

        public bool CanWrite { get; }

        public ConstrainedMemoryStreamSource(bool canSeek, bool canRead, bool canWrite)
        {
            CanSeek = canSeek;
            CanRead = canRead;
            CanWrite = canWrite;
        }

        /// <inheritdoc/>
        public MemoryStream GetInMemoryStream()
        {
            return new ConstrainedMemoryStream(CanSeek, CanRead, CanWrite);
        }
    }
}