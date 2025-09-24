namespace SecureFolderFS.Tests.Models
{
    public sealed class ConstrainedMemoryStream : MemoryStream
    {
        public override bool CanSeek { get; }

        public override bool CanRead { get; }

        public override bool CanWrite { get; }

        public ConstrainedMemoryStream(bool canSeek, bool canRead, bool canWrite)
        {
            CanSeek = canSeek;
            CanRead = canRead;
            CanWrite = canWrite;
        }

        // TODO: Add properties
    }
}
