using System;

namespace SecureFolderFS.Core.Helpers
{
    internal sealed class ReferenceBuffer
    {
        private readonly byte[] _buffer;

        public ReferenceBuffer(byte[] buffer)
        {
            _buffer = buffer;
        }

        public void Clear()
        {
            Array.Clear(_buffer);
        }

        public static implicit operator Span<byte>(ReferenceBuffer referenceBuffer) => referenceBuffer._buffer;

        public static implicit operator ReadOnlySpan<byte>(ReferenceBuffer referenceBuffer) => referenceBuffer._buffer;
    }
}
