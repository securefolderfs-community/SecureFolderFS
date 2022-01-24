using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class Rfc3394KeyWrap : IRfc3394KeyWrap
    {
        private bool _disposed;

        public byte[] Rfc3394WrapKey(byte[] bytes, byte[] kek)
        {
            AssertNotDisposed();

            return RFC3394.KeyWrapAlgorithm.WrapKey(kek: kek, plaintext: bytes);
        }

        public byte[] Rfc3394UnwrapKey(byte[] bytes, byte[] kek)
        {
            AssertNotDisposed();

            return RFC3394.KeyWrapAlgorithm.UnwrapKey(kek: kek, ciphertext: bytes);
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
