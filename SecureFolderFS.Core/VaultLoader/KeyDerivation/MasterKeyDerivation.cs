using System.Security.Cryptography;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.VaultLoader.KeyDerivation
{
    internal sealed class MasterKeyDerivation : IMasterKeyDerivation
    {
        public MasterKey DeriveMasterKey(IPassword password, BaseVaultKeystore vaultKeystore, IKeyCryptor keyCryptor)
        {
            using (password)
            {
                try
                {
                    using SecretKey kek = new SecretKey(keyCryptor.Argon2idCrypt.Argon2idHash(password.GetPassword(), vaultKeystore.salt));
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
