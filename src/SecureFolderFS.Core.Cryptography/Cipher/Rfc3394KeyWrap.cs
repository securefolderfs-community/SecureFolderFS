using RFC3394;
using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    // TODO: Needs docs
    public sealed class Rfc3394KeyWrap : IDisposable
    {
        private readonly RFC3394Algorithm _rfc3394;

        public Rfc3394KeyWrap()
        {
            _rfc3394 = new();
        }

        public byte[] WrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek)
        {
            return _rfc3394.Wrap(kek: kek.ToArray(), plainKey: bytes.ToArray());
        }

        public void UnwrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek, Span<byte> result)
        {
            var result2 = _rfc3394.Unwrap(kek: kek.ToArray(), wrappedKey: bytes.ToArray());
            result2.CopyTo(result);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _rfc3394.Dispose();
        }
    }
}
