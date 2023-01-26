using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class HeaderBuffer : BufferHolder
    {
        /// <summary>
        /// Gets or sets the value that determines whether the header buffer is initialized or not.
        /// </summary>
        public bool IsHeaderReady { get; set; }

        public HeaderBuffer(byte[] buffer)
            : base(buffer)
        {
        }

        public HeaderBuffer(int bufferLength)
            : base(bufferLength)
        {
        }
    }
}
