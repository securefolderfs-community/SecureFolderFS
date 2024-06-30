using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// Represents a message that is emitted when a vault is unlocked.
    /// </summary>
    public sealed class VaultUnlockedMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// Gets the <see cref="IVaultModel"/> of the unlocked vault.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
