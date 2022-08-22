using System;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.Security.Cipher
{
    internal sealed class KeyCryptFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly IEncryptionAlgorithmBuilder _encryptionAlgorithmBuilder;

        public KeyCryptFactory(VaultVersion vaultVersion, IEncryptionAlgorithmBuilder encryptionAlgorithmBuilder)
        {
            _vaultVersion = vaultVersion;
            _encryptionAlgorithmBuilder = encryptionAlgorithmBuilder;
        }

        public ICipherProvider GetKeyCryptor()
        {
            if (_encryptionAlgorithmBuilder is not EncryptionAlgorithmBuilder encryptionAlgorithmBuilderImpl)
            {
                throw new ArgumentException($"Provided {nameof(IEncryptionAlgorithmBuilder)} contains wrong implementation.");
            }

            if (_vaultVersion.SupportsVersion(VaultVersion.V1))
            {
                return new CipherProvider()
                {
                    Argon2idCrypt = encryptionAlgorithmBuilderImpl.Argon2idCrypt,
                    XChaCha20Poly1305Crypt = encryptionAlgorithmBuilderImpl.XChaCha20Poly1305Crypt,
                    AesGcmCrypt = encryptionAlgorithmBuilderImpl.AesGcmCrypt,
                    AesCtrCrypt = encryptionAlgorithmBuilderImpl.AesCtrCrypt,
                    AesSivCrypt = encryptionAlgorithmBuilderImpl.AesSivCrypt,
                    Rfc3394KeyWrap = encryptionAlgorithmBuilderImpl.Rfc3394KeyWrap
                };
            }

            throw new UnsupportedVaultException(_vaultVersion, GetType().Name);
        }
    }
}
