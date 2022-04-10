using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.Exceptions;

namespace SecureFolderFS.Core.VaultLoader.KeyDerivation
{
    internal sealed class MasterKeyDerivation : IMasterKeyDerivation
    {
        public MasterKey DeriveMasterKey(DisposablePassword disposablePassword, BaseVaultKeystore vaultKeystore, IKeyCryptor keyCryptor)
        {
            using (disposablePassword)
            {
                try
                {
                    if (disposablePassword?.Password?.Bytes?.IsEmpty() ?? true)
                    {
                        throw new ArgumentException($"The password was empty or null.");
                    }

                    using SecretKey kek = new SecretKey(keyCryptor.Argon2idCrypt.Argon2idHash(disposablePassword.Password, vaultKeystore.salt));
                    using SecretKey encKey = new SecretKey(keyCryptor.Rfc3394KeyWrap.Rfc3394UnwrapKey(vaultKeystore.wrappedEncryptionKey, kek));
                    using SecretKey macKey = new SecretKey(keyCryptor.Rfc3394KeyWrap.Rfc3394UnwrapKey(vaultKeystore.wrappedMacKey, kek));

                    return MasterKey.Create(encKey, macKey);
                }
                catch (CryptographicException)
                {
                    throw new IncorrectPasswordException();
                }
            }
        }
    }
}
