using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed class VaultDashboardPageViewModel : BaseVaultPageViewModel
    {
        public VaultViewModel VaultViewModel { get; }

        public VaultDashboardPageViewModel(IUnlockedVaultModel unlockedVaultModel, IVaultModel vaultModel, IMessenger messenger)
            : base(vaultModel, messenger)
        {
            VaultViewModel = new(unlockedVaultModel, vaultModel);
        }
    }
}
