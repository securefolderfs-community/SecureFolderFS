using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public sealed class VaultPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public VaultPropertiesPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationModel<DashboardPageType> navigationModel)
            : base(unlockedVaultViewModel, navigationModel)
        {
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
