using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.Cipher.Default;
using System;

namespace SecureFolderFS.Core.Cryptography
{
    /// <summary>
    /// Provides cipher algorithm implementations.
    /// </summary>
    public sealed class CipherProvider : IDisposable
    {
        // TODO: Needs docs

        public required IArgon2idCrypt Argon2idCrypt { get; init; }

        public required IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; init; }

        public required IAesGcmCrypt AesGcmCrypt { get; init; }

        public required IAesCtrCrypt AesCtrCrypt { get; init; }

        public required IAesSivCrypt AesSivCrypt { get; init; }

        public required IRfc3394KeyWrap Rfc3394KeyWrap { get; init; }

        public required IHmacSha256Crypt HmacSha256Crypt { get; init; }

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

        /// <inheritdoc/>
        public void Dispose()
        {
            AesSivCrypt.Dispose();
        }
    }
}
