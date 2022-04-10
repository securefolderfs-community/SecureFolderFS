using System;
using NSec.Cryptography;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class XChaCha20Poly1305Crypt : IXChaCha20Poly1305Crypt
    {
        private bool _disposed;

        public int XChaCha20Poly1305TagSize { get; } = Constants.Security.EncryptionAlgorithm.XChaCha20.XCHACHA20_POLY1305_TAG_SIZE;

        public byte[] XChaCha20Poly1305Encrypt(byte[] bytes, byte[] key, byte[] nonce, out byte[] tag, byte[] associatedData = null)
        {
            AssertNotDisposed();

            using var key2 = NSec.Cryptography.Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);

            var ciphertextWithTag = AeadAlgorithm.XChaCha20Poly1305.Encrypt(key2, nonce, associatedData, bytes);

            var ciphertext = ciphertextWithTag.Slice(0, ciphertextWithTag.Length - XChaCha20Poly1305TagSize);
            tag = ciphertextWithTag.Slice(ciphertext.Length, XChaCha20Poly1305TagSize);

            return ciphertext;
        }

        public byte[] XChaCha20Poly1305Decrypt(byte[] bytes, byte[] key, byte[] nonce, byte[] tag, byte[] associatedData = null)
        {
            AssertNotDisposed();

            using var key2 = NSec.Cryptography.Key.Import(AeadAlgorithm.XChaCha20Poly1305, key, KeyBlobFormat.RawSymmetricKey);

            byte[] ciphertextWithTag = new byte[bytes.Length + tag.Length];
            ciphertextWithTag.EmplaceArrays(bytes, tag);

            var cleartext = AeadAlgorithm.XChaCha20Poly1305.Decrypt(key2, nonce, associatedData, ciphertextWithTag);

            return cleartext;
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
