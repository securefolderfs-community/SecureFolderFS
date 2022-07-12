using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public abstract class BaseVaultPageViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        protected IMessenger Messenger { get; }

        public IVaultModel VaultModel { get; }

        protected BaseVaultPageViewModel(IVaultModel vaultModel, IMessenger messenger)
        {
            VaultModel = vaultModel;
            Messenger = messenger;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}
