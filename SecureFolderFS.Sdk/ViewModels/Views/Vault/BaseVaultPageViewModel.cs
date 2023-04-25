using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    /// <inheritdoc cref="BasePageViewModel"/>
    public abstract class BaseVaultPageViewModel : BasePageViewModel, IDisposable
    {
        /// <summary>
        /// Gets the view model of the vault.
        /// </summary>
        public VaultViewModel VaultViewModel { get; }

        /// <summary>
        /// Gets the <see cref="INavigationService"/> that controls the navigation of vault pages.
        /// </summary>
        public INavigationService NavigationService { get; }

        protected BaseVaultPageViewModel(VaultViewModel vaultViewModel, INavigationService navigationService)
        {
            VaultViewModel = vaultViewModel;
            NavigationService = navigationService;
        }

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
