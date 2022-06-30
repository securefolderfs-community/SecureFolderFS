using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Vault
{
    public sealed class VaultHealthViewModel
    {
        private IVaultHealthModel VaultHealthModel { get; }

        public VaultHealthViewModel(IVaultHealthModel vaultHealthModel)
        {
            VaultHealthModel = vaultHealthModel;
        }
    }
}
