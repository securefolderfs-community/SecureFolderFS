using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class ChunkBuffer : BufferHolder
    {
        /// <summary>
        /// Gets or sets the value that determines whether the chunk has been modified or not.
        /// </summary>
        public bool WasModified { get; set; }

        /// <summary>
        /// Gets or sets the actual length of filled bytes in the whole buffer.
        /// </summary>
        public int ActualLength { get; set; }

        public ChunkBuffer(byte[] buffer, int actualLength)
            : base(buffer)
        {
            ActualLength = actualLength;
        }
    }
}
