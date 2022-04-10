using System;
using Miscreant;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class AesSivCrypt : IAesSivCrypt
    {
        private bool _disposed;

        // TODO: Check correctness of passed parameters

        public byte[] AesSivEncrypt(byte[] cleartextBytes, byte[] encryptionKey, byte[] macKey, byte[] associatedData)
        {
            AssertNotDisposed();

            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey, macKey);

            // The longKey will be split into two keys - one for S2V and the other one for CTR

            using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            return aesCmacSiv.Seal(cleartextBytes, data: associatedData);
        }

        public byte[] AesSivDecrypt(byte[] ciphertextBytes, byte[] encryptionKey, byte[] macKey, byte[] associatedData)
        {
            AssertNotDisposed();

            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey, macKey);

            // The longKey will be split into two keys - one for S2V and the other one for CTR

            using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            return aesCmacSiv.Open(ciphertextBytes, data: associatedData);
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
