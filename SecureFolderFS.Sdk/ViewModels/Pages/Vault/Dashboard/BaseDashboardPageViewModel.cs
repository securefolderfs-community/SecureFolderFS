using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, IAsyncInitialize
    {
        protected IMessenger Messenger { get; }

        protected VaultViewModel VaultViewModel { get; }

        protected BaseDashboardPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
        {
            Messenger = messenger;
            VaultViewModel = vaultViewModel;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
