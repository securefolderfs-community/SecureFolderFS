using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Sdk.ViewModels.Dashboard;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public sealed class VaultOverviewPageViewModel : BaseDashboardPageViewModel
    {
        public WidgetsListViewModel WidgetsViewModel { get; }

        public VaultControlsViewModel VaultControlsViewModel { get; }

        public VaultOverviewPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
            : base(messenger, vaultViewModel)
        {
            WidgetsViewModel = new();
            VaultControlsViewModel = new(messenger, vaultViewModel);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await WidgetsViewModel.InitAsync(cancellationToken);
        }
    }
}
