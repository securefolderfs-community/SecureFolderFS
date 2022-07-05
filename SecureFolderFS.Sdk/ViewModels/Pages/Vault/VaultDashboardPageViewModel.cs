using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed class VaultDashboardPageViewModel : BaseVaultPageViewModel, IAsyncInitialize
    {
        public VaultViewModel VaultViewModel { get; }

        public BaseDashboardPageViewModel CurrentPage { get; }

        public VaultDashboardPageViewModel(IUnlockedVaultModel unlockedVaultModel, IVaultModel vaultModel, IMessenger messenger)
            : base(vaultModel, messenger)
        {
            VaultViewModel = new(unlockedVaultModel, vaultModel);
            CurrentPage = new VaultOverviewPageViewModel(Messenger, VaultViewModel);
        }

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await CurrentPage.InitAsync(cancellationToken);
        }
    }
}
