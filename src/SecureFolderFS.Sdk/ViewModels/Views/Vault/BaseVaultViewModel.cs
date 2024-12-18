using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    /// <inheritdoc cref="IViewDesignation"/>
    public abstract partial class BaseVaultViewModel(VaultViewModel vaultViewModel)
        : BaseDesignationViewModel, IAsyncInitialize, IDisposable
    {
        /// <summary>
        /// Gets the vault view model associated with the vault.
        /// </summary>
        public VaultViewModel VaultViewModel { get; } = vaultViewModel;

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
