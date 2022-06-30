using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed class VaultDashboardPageViewModel : BaseVaultPageViewModel, IAsyncInitialize
    {
        private VaultViewModel VaultViewModel { get; }

        public VaultDashboardPageViewModel(VaultViewModel vaultViewModel, IMessenger messenger, IVaultModel vaultModel)
            : base(messenger, vaultModel)
        {
            VaultViewModel = vaultViewModel;
        }

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (VaultViewModel.VaultInstance is null)
                return;

            await Task.Run(() =>
            {
                VaultViewModel.VaultInstance.SecureFolderFSInstance.StartFileSystem();
            });
        }
    }
}
