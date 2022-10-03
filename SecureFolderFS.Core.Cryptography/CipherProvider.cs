using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.CryptImpl;

namespace SecureFolderFS.Core.Cryptography
{
    /// <summary>
    /// Provides cipher algorithm implementations.
    /// </summary>
    public sealed class CipherProvider
    {
        // TODO: Needs docs
        // TODO: Add required modifier

        public IArgon2idCrypt Argon2idCrypt { get; private init; }

        public IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; private init; }

        public IAesGcmCrypt AesGcmCrypt { get; private init; }

        public IAesCtrCrypt AesCtrCrypt { get; private init; }

        public IAesSivCrypt AesSivCrypt { get; private init; }

        public IRfc3394KeyWrap Rfc3394KeyWrap { get; private init; }

        public IHmacSha256Crypt HmacSha256Crypt { get; private init; }

        public static CipherProvider CreateNew()
        {
            return new()
            {
                Argon2idCrypt = new Argon2idCrypt(),
                XChaCha20Poly1305Crypt = new XChaCha20Poly1305Crypt(),
                AesGcmCrypt = new AesGcmCrypt(),
                AesCtrCrypt = new AesCtrCrypt(),
                AesSivCrypt = new AesSivCrypt(),
                Rfc3394KeyWrap = new Rfc3394KeyWrap(),
                HmacSha256Crypt = new HmacSha256Crypt()
            };
        }
    }
}
