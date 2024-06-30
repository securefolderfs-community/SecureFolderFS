using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message to remove a vault.
    /// </summary>
    public sealed class RemoveVaultMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// The vault to remove.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
