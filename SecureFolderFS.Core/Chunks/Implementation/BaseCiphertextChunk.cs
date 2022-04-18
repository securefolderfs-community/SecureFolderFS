using System;

namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal abstract class BaseCiphertextChunk : ICiphertextChunk
    {
        public ReadOnlyMemory<byte> Buffer { get; }

        protected BaseCiphertextChunk(ReadOnlyMemory<byte> buffer)
        {
            this.Buffer = buffer;
        }

        public abstract ReadOnlySpan<byte> GetNonceAsSpan();

        public abstract ReadOnlySpan<byte> GetPayloadAsSpan();

        public abstract ReadOnlySpan<byte> GetAuthAsSpan();
    }
}
