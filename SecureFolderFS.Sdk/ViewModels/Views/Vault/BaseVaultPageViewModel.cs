using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public abstract class BaseVaultPageViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        public VaultViewModel VaultViewModel { get; }

        protected BaseVaultPageViewModel(VaultViewModel vaultViewModel)
        {
            VaultViewModel = vaultViewModel;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
