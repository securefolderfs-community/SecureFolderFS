using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed class VaultDashboardPageViewModel : BaseVaultPageViewModel
    {
        public BaseDashboardPageViewModel CurrentPage { get; }

        public VaultDashboardPageViewModel(VaultViewModel vaultViewModel, IMessenger messenger)
            : base(vaultViewModel.VaultModel, messenger)
        {
            CurrentPage = new VaultOverviewPageViewModel(vaultViewModel, messenger);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await CurrentPage.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            CurrentPage.Dispose();
        }
    }
}
