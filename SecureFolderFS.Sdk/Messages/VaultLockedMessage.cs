using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// Represents a message that is emitted when a vault is locked.
    /// </summary>
    public sealed class VaultLockedMessage : IMessage
    {
        /// <summary>
        /// Gets the <see cref="IVaultModel"/> of the locked vault.
        /// </summary>
        public IVaultModel VaultModel { get; }

        public VaultLockedMessage(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }
    }
}
