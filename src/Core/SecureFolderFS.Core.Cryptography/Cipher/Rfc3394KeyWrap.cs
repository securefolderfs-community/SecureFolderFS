using RFC3394;
using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public sealed class Rfc3394KeyWrap : IDisposable
    {
        private readonly RFC3394Algorithm? _rfc3394;

        public Rfc3394KeyWrap()
        {
            _rfc3394 = Constants.PreferBouncyCastle ? null : new();
        }

        public byte[] WrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek)
        {
            if (_rfc3394 is not null)
                return _rfc3394.Wrap(kek: kek.ToArray(), plainKey: bytes.ToArray());

            var engine = new AesWrapEngine();
            engine.Init(true, new KeyParameter(kek.ToArray()));
            var plain = bytes.ToArray();
            return engine.Wrap(plain, 0, plain.Length);
        }

        public void UnwrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek, Span<byte> result)
        {
            if (_rfc3394 is not null)
            {
                var unwrapped = _rfc3394.Unwrap(kek: kek.ToArray(), wrappedKey: bytes.ToArray());
                unwrapped.CopyTo(result);
                return;
            }

            var engine = new AesWrapEngine();
            engine.Init(false, new KeyParameter(kek.ToArray()));
            var wrapped = bytes.ToArray();
            try
            {
                var unwrapped = engine.Unwrap(wrapped, 0, wrapped.Length);
                unwrapped.CopyTo(result);
            }
            catch (InvalidCipherTextException ex)
            {
                // The native RFC3394.net path throws CryptographicException on an integrity failure;
                // surface the same type so unlock's wrong-credential handling is unchanged.
                throw new CryptographicException("The wrapped key failed its integrity check.", ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _rfc3394?.Dispose();
        }
    }
}
