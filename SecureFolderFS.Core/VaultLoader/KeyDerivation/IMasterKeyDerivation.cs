using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;
using SecureFolderFS.Core.PasswordRequest;

namespace SecureFolderFS.Core.VaultLoader.KeyDerivation
{
    /// <summary>
    /// Provides module for deriving master key.
    /// </summary>
    internal interface IMasterKeyDerivation
    {
        MasterKey DeriveMasterKey(DisposablePassword disposablePassword, BaseVaultKeystore vaultKeystore, IKeyCryptor keyCryptor);
    }
}
