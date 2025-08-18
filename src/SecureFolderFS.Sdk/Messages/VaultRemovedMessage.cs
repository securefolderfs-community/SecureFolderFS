using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message notifying a vault was removed.
    /// </summary>
    public sealed class VaultRemovedMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// The vault that was removed.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
