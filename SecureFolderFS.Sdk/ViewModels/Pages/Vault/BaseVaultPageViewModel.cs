using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public abstract class BaseVaultPageViewModel : ObservableObject, IDisposable
    {
        protected IVaultModel VaultModel { get; }

        protected IMessenger Messenger { get; }

        protected BaseVaultPageViewModel(IVaultModel vaultModel, IMessenger messenger)
        {
            VaultModel = vaultModel;
            Messenger = messenger;
        }

        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}
