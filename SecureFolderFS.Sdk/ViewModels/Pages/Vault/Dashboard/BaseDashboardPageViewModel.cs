using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        protected VaultViewModel VaultViewModel { get; }

        protected IMessenger Messenger { get; }

        protected BaseDashboardPageViewModel(VaultViewModel vaultViewModel, IMessenger messenger)
        {
            VaultViewModel = vaultViewModel;
            Messenger = messenger;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}
