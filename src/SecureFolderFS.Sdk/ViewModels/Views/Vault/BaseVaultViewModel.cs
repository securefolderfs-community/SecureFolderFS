using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    /// <inheritdoc cref="IViewDesignation"/>
    public abstract partial class BaseVaultViewModel(IVaultModel vaultModel)
        : BaseDesignationViewModel, IAsyncInitialize, IDisposable
    {
        /// <summary>
        /// Gets the vault model associated with the vault.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
