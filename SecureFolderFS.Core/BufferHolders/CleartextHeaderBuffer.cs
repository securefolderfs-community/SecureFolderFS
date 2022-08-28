namespace SecureFolderFS.Core.BufferHolders
{
    internal sealed class CleartextHeaderBuffer : BaseBufferHolder
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
