using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message requesting that a specific vault be selected in the UI.
    /// </summary>
    public sealed class VaultSelectionRequestedMessage(IVaultModel vaultModel)
    {
        /// <summary>
        /// Gets the vault that should be selected.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}

