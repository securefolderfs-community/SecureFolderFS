using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public abstract class BaseVaultPageViewModel : ObservableObject, IDisposable
    {
        protected IMessenger Messenger { get; }

        protected IVaultModel VaultModel { get; }

        protected BaseVaultPageViewModel(IMessenger messenger, IVaultModel vaultModel)
        {
            Messenger = messenger;
            VaultModel = vaultModel;
        }

        public virtual void Dispose() { }
    }
}
