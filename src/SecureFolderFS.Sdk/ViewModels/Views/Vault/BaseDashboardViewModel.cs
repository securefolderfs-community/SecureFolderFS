using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    /// <inheritdoc cref="IViewDesignation"/>
    public abstract class BaseDashboardViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
        : BaseVaultViewModel(unlockedVaultViewModel.VaultViewModel)
    {
        /// <summary>
        /// Gets the view model of the unlocked vault.
        /// </summary>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; } = unlockedVaultViewModel;
    }
}
