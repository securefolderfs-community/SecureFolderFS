using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.BufferHolders
{
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
