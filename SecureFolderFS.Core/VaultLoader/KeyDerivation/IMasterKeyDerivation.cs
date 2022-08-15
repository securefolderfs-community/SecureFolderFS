using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.VaultLoader.KeyDerivation
{
    /// <summary>
    /// Provides module for deriving master key.
    /// </summary>
    internal interface IMasterKeyDerivation
    {
        MasterKey DeriveMasterKey(IPassword password, BaseVaultKeystore vaultKeystore, IKeyCryptor keyCryptor);
    }
}
