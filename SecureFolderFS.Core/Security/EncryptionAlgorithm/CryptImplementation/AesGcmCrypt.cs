using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class AesGcmCrypt : IAesGcmCrypt
    {
        private bool _disposed;

        public int AesGcmTagSize { get; } = Constants.Security.EncryptionAlgorithm.AesGcm.AES_GCM_TAG_SIZE; // AesGcm.TagByteSizes.MaxSize = 16

        public byte[] AesGcmEncrypt(byte[] bytes, byte[] key, byte[] iv, out byte[] tag, byte[] associatedData = null)
        {
            AssertNotDisposed();
            
            tag = new byte[AesGcmTagSize];
            var result = new byte[bytes.Length];

            using var aesGcm = new AesGcm(key);
            aesGcm.Encrypt(nonce: iv, plaintext: bytes, ciphertext: result, tag: tag, associatedData: associatedData);

            return result;
        }

        public void AesGcmEncrypt2(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData = default)
        {
            AssertNotDisposed();

            using var aesGcm = new AesGcm(key);
            aesGcm.Encrypt(nonce: nonce, plaintext: bytes, ciphertext: result, tag: tag, associatedData: associatedData);
        }

        public byte[] AesGcmDecrypt(byte[] bytes, byte[] key, byte[] iv, byte[] tag, byte[] associatedData = null)
        {
            AssertNotDisposed();

            var result = new byte[bytes.Length];

            using var aesGcm = new AesGcm(key);
            aesGcm.Decrypt(nonce: iv, ciphertext: bytes, tag: tag, plaintext: result, associatedData: associatedData);

            return result;
        }

        public void AesGcmDecrypt2(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData = default)
        {
            AssertNotDisposed();

            using var aesGcm = new AesGcm(key);
            aesGcm.Decrypt(nonce: iv, ciphertext: bytes, tag: tag, plaintext: result, associatedData: associatedData);
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
