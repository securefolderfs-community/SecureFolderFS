using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed class VaultDashboardPageViewModel : BaseVaultPageViewModel, IRecipient<VaultLockedMessage>
    {
        public IStateNavigationModel NavigationModel { get; }

        public BaseDashboardPageViewModel CurrentPage { get; }

        public VaultDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IMessenger messenger)
            : base(unlockedVaultViewModel.VaultViewModel, messenger)
        {
            NavigationModel = new DashboardNavigationModel(messenger);
            CurrentPage = new VaultOverviewPageViewModel(unlockedVaultViewModel, NavigationModel);

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await CurrentPage.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            // Free resources that are used by the dashboard
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                Dispose();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            CurrentPage.Dispose();
        }
    }
}
