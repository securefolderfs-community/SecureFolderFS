using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message to add a vault.
    /// </summary>
    public sealed class AddVaultMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// The vault to add.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
