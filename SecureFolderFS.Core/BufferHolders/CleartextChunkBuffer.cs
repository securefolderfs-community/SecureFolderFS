namespace SecureFolderFS.Core.BufferHolders
{
    internal sealed class CleartextChunkBuffer : BaseBufferHolder
    {
        public long ActualLength { get; set; }

        public CleartextChunkBuffer(byte[] buffer)
            : base(buffer)
        {
        }

        public CleartextChunkBuffer(int bufferLength)
            : base(bufferLength)
        {
        }
    }
}
