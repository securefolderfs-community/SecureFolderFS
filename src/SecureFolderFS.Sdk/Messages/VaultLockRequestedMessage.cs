using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// Represents a message that is emitted to request locking of a vault.
    /// </summary>
    public sealed class VaultLockRequestedMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// Gets the <see cref="IVaultModel"/> of the locked vault.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
