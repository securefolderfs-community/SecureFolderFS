using SecureFolderFS.Core.Cryptography.Cipher;
using System;

namespace SecureFolderFS.Core.Cryptography.CryptImpl
{
    /// <inheritdoc cref="IRfc3394KeyWrap"/>
    public sealed class Rfc3394KeyWrap : IRfc3394KeyWrap
    {
        /// <inheritdoc/>
        public byte[] WrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek)
        {
            return RFC3394.KeyWrapAlgorithm.WrapKey(kek: kek.ToArray(), plaintext: bytes.ToArray());
        }

        /// <inheritdoc/>
        public void UnwrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek, Span<byte> result)
        {
            var result2 = RFC3394.KeyWrapAlgorithm.UnwrapKey(kek: kek.ToArray(), ciphertext: bytes.ToArray());
            result2.CopyTo(result);
        }
    }
}
