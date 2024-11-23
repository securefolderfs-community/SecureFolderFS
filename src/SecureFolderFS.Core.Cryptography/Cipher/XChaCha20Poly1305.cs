using System;
using System.Collections.Specialized;
using Sodium;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    // TODO: Needs docs
    public static class XChaCha20Poly1305
    {
        public static void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> resultAndTag, ReadOnlySpan<byte> associatedData)
        {
            var encrypted = SecretAeadXChaCha20Poly1305.Encrypt(bytes.ToArray(), nonce.ToArray(), key.ToArray(), associatedData.ToArray());
            encrypted.CopyTo(resultAndTag);
            
            return;
            
            // using var key2 = Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);
            // AeadAlgorithm.XChaCha20Poly1305.Encrypt(key2, nonce, associatedData, bytes, resultAndTag);
        }

        public static bool Decrypt(ReadOnlySpan<byte> bytesWithTag, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            var decrypted = SecretAeadXChaCha20Poly1305.Decrypt(bytesWithTag.ToArray(), nonce.ToArray(), key.ToArray(), associatedData.ToArray());
            decrypted.CopyTo(result);
            
            return true;
            
            // using var key2 = Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);
            // return AeadAlgorithm.XChaCha20Poly1305.Decrypt(key2, nonce, associatedData, bytesWithTag, result);
        }
    }
}
