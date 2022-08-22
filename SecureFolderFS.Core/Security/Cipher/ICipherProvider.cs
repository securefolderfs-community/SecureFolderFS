using SecureFolderFS.Core.Security.EncryptionAlgorithm;

namespace SecureFolderFS.Core.Security.Cipher
{
    internal interface ICipherProvider
    {
        IArgon2idCrypt Argon2idCrypt { get; }

        IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; }

        IAesGcmCrypt AesGcmCrypt { get; }

        IAesCtrCrypt AesCtrCrypt { get; }

        IAesSivCrypt AesSivCrypt { get; }

        IRfc3394KeyWrap Rfc3394KeyWrap { get; }

        IHmacSha256Crypt GetHmacInstance();
    }
}
