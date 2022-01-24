using SecureFolderFS.Core.Security.EncryptionAlgorithm;

namespace SecureFolderFS.Core.Security.KeyCrypt
{
    internal sealed class KeyCryptor : IKeyCryptor
    {
        private bool _disposed;

        public IArgon2idCrypt Argon2idCrypt { get; init; }

        public IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; init; }

        public IAesGcmCrypt AesGcmCrypt { get; init; }

        public IAesCtrCrypt AesCtrCrypt { get; init; }

        public IAesSivCrypt AesSivCrypt { get; init; }

        public IHmacSha256Crypt HmacSha256Crypt { get; init; }

        public IRfc3394KeyWrap Rfc3394KeyWrap { get; init; }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Argon2idCrypt?.Dispose();
                XChaCha20Poly1305Crypt?.Dispose();
                AesGcmCrypt?.Dispose();
                AesCtrCrypt?.Dispose();
                AesSivCrypt?.Dispose();
                HmacSha256Crypt?.Dispose();
                Rfc3394KeyWrap?.Dispose();
            }
        }
    }
}
