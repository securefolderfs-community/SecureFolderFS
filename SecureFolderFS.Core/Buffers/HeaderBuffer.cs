using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class HeaderBuffer : BufferHolder
    {
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
