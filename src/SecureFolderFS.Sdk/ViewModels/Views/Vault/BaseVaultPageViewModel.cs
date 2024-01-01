using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    /// <inheritdoc cref="BasePageViewModel"/>
    public abstract class BaseVaultPageViewModel(VaultViewModel vaultViewModel, INavigationService navigationService)
        : BasePageViewModel, IDisposable
    {
        /// <summary>
        /// Gets the view model of the vault.
        /// </summary>
        public VaultViewModel VaultViewModel { get; } = vaultViewModel;

        /// <summary>
        /// Gets the <see cref="INavigationService"/> that controls the navigation of vault pages.
        /// </summary>
        public INavigationService NavigationService { get; } = navigationService;

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
