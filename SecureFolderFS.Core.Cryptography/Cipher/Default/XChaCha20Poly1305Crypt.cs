using NSec.Cryptography;
using System;

namespace SecureFolderFS.Core.Cryptography.Cipher.Default
{
    /// <inheritdoc cref="IXChaCha20Poly1305Crypt"/>
    public sealed class XChaCha20Poly1305Crypt : IXChaCha20Poly1305Crypt
    {
        /// <inheritdoc/>
        public void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> resultAndTag, ReadOnlySpan<byte> associatedData)
        {
            using var key2 = Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);
            AeadAlgorithm.XChaCha20Poly1305.Encrypt(key2, nonce, associatedData, bytes, resultAndTag);
        }

        /// <inheritdoc/>
        public bool Decrypt(ReadOnlySpan<byte> bytesWithTag, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            using var key2 = Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);
            return AeadAlgorithm.XChaCha20Poly1305.Decrypt(key2, nonce, associatedData, bytesWithTag, result);
        }
    }
}
