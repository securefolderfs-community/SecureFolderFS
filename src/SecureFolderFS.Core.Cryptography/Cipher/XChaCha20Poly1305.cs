using System;
using NSec.Cryptography;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    // TODO: Needs docs
    public static class XChaCha20Poly1305
    {
        public static void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> resultAndTag, ReadOnlySpan<byte> associatedData)
        {
            using var key2 = Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);
            AeadAlgorithm.XChaCha20Poly1305.Encrypt(key2, nonce, associatedData, bytes, resultAndTag);
        }

        public static bool Decrypt(ReadOnlySpan<byte> bytesWithTag, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            using var key2 = Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);
            return AeadAlgorithm.XChaCha20Poly1305.Decrypt(key2, nonce, associatedData, bytesWithTag, result);
        }
    }
}
