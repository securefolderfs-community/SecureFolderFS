using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public abstract class BaseVaultPageViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        public IMessenger Messenger { get; }

        public VaultViewModel VaultViewModel { get; }

        protected BaseVaultPageViewModel(VaultViewModel vaultViewModel, IMessenger messenger)
        {
            VaultViewModel = vaultViewModel;
            Messenger = messenger;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
