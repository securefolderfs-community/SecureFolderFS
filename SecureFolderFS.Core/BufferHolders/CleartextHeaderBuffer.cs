using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.BufferHolders
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class CleartextHeaderBuffer : BufferHolder
    {
        public bool IsHeaderReady { get; set; }

        public CleartextHeaderBuffer(byte[] buffer)
            : base(buffer)
        {
        }

        public CleartextHeaderBuffer(int bufferLength)
            : base(bufferLength)
        {
        }
    }
}
