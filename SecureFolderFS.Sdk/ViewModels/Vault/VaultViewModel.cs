using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Vault
{
    public class VaultViewModel : ObservableObject
    {
        public IVaultModel VaultModel { get; }

        public VaultHealthViewModel VaultHealthViewModel { get; }

        public string VaultName { get; }

        public VaultViewModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
            VaultHealthViewModel = new();
            VaultName = vaultModel.Folder.Name;
        }
    }
}
