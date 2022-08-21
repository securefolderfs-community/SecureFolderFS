using SecureFolderFS.Core.Security.EncryptionAlgorithm;

namespace SecureFolderFS.Core.Security.Cipher
{
    internal sealed class CipherProvider : ICipherProvider
    {
        private bool _disposed;

        public IArgon2idCrypt Argon2idCrypt { get; init; }

        public IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; init; }

        public IAesGcmCrypt AesGcmCrypt { get; init; }

        public IAesCtrCrypt AesCtrCrypt { get; init; }

        public IAesSivCrypt AesSivCrypt { get; init; }

        public IHmacSha256Crypt HmacSha256Crypt { get; init; } // TODO: OPTIMIZE - use GetHmacInstance();

        public IRfc3394KeyWrap Rfc3394KeyWrap { get; init; }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                HmacSha256Crypt?.Dispose();
            }
        }
    }
}
