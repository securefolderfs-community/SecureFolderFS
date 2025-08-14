using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message notifying a vault was added.
    /// </summary>
    public sealed class VaultAddedMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// The vault that was added.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
