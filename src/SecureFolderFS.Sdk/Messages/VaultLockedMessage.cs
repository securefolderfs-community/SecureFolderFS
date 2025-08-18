using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// Represents a message emitted when a vault is locked.
    /// </summary>
    public sealed class VaultLockedMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// Gets the <see cref="IVaultModel"/> of the vault that was locked.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
