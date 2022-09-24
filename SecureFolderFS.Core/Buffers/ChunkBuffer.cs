using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class ChunkBuffer : BufferHolder
    {
        public bool IsDirty { get; set; }

        public int ActualLength { get; set; }

        public ChunkBuffer(byte[] buffer, int actualLength)
            : base(buffer)
        {
            ActualLength = actualLength;
        }
    }
}
