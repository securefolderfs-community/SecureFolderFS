using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public sealed class VaultPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public VaultPropertiesPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IMessenger messenger)
            : base(unlockedVaultViewModel, messenger)
        {
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
