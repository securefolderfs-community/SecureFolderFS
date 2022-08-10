using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public sealed class VaultPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public VaultPropertiesPageViewModel(VaultViewModel vaultViewModel, IMessenger messenger)
            : base(vaultViewModel, messenger)
        {
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
