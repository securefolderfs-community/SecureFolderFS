using SecureFolderFS.Core.Security.EncryptionAlgorithm;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation;

namespace SecureFolderFS.Core.Security.Cipher
{
    internal sealed class CipherProvider : ICipherProvider
    {
        public IArgon2idCrypt Argon2idCrypt { get; init; }

        public IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; init; }

        public IAesGcmCrypt AesGcmCrypt { get; init; }

        public IAesCtrCrypt AesCtrCrypt { get; init; }

        public IAesSivCrypt AesSivCrypt { get; init; }

        public IRfc3394KeyWrap Rfc3394KeyWrap { get; init; }

        public IHmacSha256Crypt GetHmacInstance()
        {
            return new HmacSha256Crypt();
        }
    }
}
