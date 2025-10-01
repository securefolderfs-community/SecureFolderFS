using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.FileSystem.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    public sealed class HeaderBuffer : BufferHolder
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
