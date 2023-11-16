using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message to add a vault.
    /// </summary>
    public sealed class AddVaultMessage
    {
        /// <summary>
        /// The vault to add.
        /// </summary>
        public IVaultModel VaultModel { get; }

        public AddVaultMessage(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }
    }
}
