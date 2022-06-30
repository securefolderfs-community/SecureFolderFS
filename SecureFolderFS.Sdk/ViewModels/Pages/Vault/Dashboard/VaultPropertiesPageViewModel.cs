using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public sealed class VaultPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public VaultPropertiesPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
            : base(messenger, vaultViewModel)
        {
        }
    }
}
