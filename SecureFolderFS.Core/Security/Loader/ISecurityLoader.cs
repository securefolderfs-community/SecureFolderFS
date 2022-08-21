using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.Security.Loader
{
    internal interface ISecurityLoader
    {
        ISecurity LoadSecurity(BaseVaultConfiguration vaultConfiguration, ICipherProvider keyCryptor, MasterKey masterKeyCopy);
    }
}
