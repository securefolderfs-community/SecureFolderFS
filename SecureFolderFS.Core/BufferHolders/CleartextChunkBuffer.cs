using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.BufferHolders
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class CleartextChunkBuffer : BufferHolder
    {
        public bool IsDirty { get; set; }

        public int ActualLength { get; set; }

        public CleartextChunkBuffer(byte[] buffer, int actualLength)
            : base(buffer)
        {
            ActualLength = actualLength;
        }
    }
}
