using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public abstract class BaseVaultPageViewModel : ObservableObject, IEquatable<BaseVaultPageViewModel>, IDisposable
    {
        protected IMessenger Messenger { get; }

        public IVaultModel VaultModel { get; }

        protected BaseVaultPageViewModel(IVaultModel vaultModel, IMessenger messenger)
        {
            VaultModel = vaultModel;
            Messenger = messenger;
        }

        /// <inheritdoc/>
        public bool Equals(BaseVaultPageViewModel? other)
        {
            return VaultModel.Equals(other?.VaultModel);
        }

        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}
