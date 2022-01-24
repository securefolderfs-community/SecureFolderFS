using System;
using SecureFolderFS.Core.Security.EncryptionAlgorithm;

namespace SecureFolderFS.Core.Security.KeyCrypt
{
    internal interface IKeyCryptor : IDisposable
    {
        IArgon2idCrypt Argon2idCrypt { get; }

        IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; }

        IAesGcmCrypt AesGcmCrypt { get; }

        IAesCtrCrypt AesCtrCrypt { get; }

        IAesSivCrypt AesSivCrypt { get; }

        IHmacSha256Crypt HmacSha256Crypt { get; }

        IRfc3394KeyWrap Rfc3394KeyWrap { get; }
    }
}
