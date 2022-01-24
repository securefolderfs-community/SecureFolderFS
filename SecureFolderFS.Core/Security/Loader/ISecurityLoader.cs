using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.Security.Loader
{
    internal interface ISecurityLoader
    {
        ISecurity LoadSecurity(BaseVaultConfiguration vaultConfiguration, IKeyCryptor keyCryptor, MasterKey masterKeyCopy);
    }
}
