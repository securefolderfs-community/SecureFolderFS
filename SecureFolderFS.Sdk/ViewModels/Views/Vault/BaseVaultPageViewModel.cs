using SecureFolderFS.Sdk.ViewModels.Vault;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public abstract class BaseVaultPageViewModel : BasePageViewModel, IDisposable
    {
        public VaultViewModel VaultViewModel { get; }

        protected BaseVaultPageViewModel(VaultViewModel vaultViewModel)
        {
            VaultViewModel = vaultViewModel;
        }

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
