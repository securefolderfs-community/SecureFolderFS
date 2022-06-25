using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message to remove a vault.
    /// </summary>
    public sealed class RemoveVaultMessage : IMessage
    {
        /// <summary>
        /// The vault to remove.
        /// </summary>
        public IVaultModel VaultModel { get; }

        public RemoveVaultMessage(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }
    }
}
