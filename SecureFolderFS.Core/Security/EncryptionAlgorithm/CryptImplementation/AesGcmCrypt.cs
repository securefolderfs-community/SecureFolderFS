using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class AesGcmCrypt : IAesGcmCrypt
    {
        public byte[] AesGcmEncryptDeprecated(byte[] bytes, byte[] key, byte[] iv, out byte[] tag, byte[] associatedData = null)
        {
            tag = new byte[Constants.Security.EncryptionAlgorithm.AesGcm.AES_GCM_TAG_SIZE];
            var result = new byte[bytes.Length];

            using var aesGcm = new AesGcm(key);
            aesGcm.Encrypt(nonce: iv, plaintext: bytes, ciphertext: result, tag: tag, associatedData: associatedData);

            return result;
        }

        public byte[] AesGcmDecryptDeprecated(byte[] bytes, byte[] key, byte[] iv, byte[] tag, byte[] associatedData = null)
        {
            var result = new byte[bytes.Length];

            using var aesGcm = new AesGcm(key);
            aesGcm.Decrypt(nonce: iv, ciphertext: bytes, tag: tag, plaintext: result, associatedData: associatedData);

            return result;
        }

        public void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            using var aesGcm = new AesGcm(key);
            aesGcm.Encrypt(nonce: nonce, plaintext: bytes, ciphertext: result, tag: tag, associatedData: associatedData);
        }

        public bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            using var aesGcm = new AesGcm(key);
            aesGcm.Decrypt(nonce: iv, ciphertext: bytes, tag: tag, plaintext: result, associatedData: associatedData);
        }
    }
}
