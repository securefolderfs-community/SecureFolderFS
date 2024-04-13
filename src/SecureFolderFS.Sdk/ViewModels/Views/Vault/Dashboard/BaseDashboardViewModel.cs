using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    /// <inheritdoc cref="IViewDesignation"/>
    public abstract class BaseDashboardViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
        : BaseVaultViewModel(unlockedVaultViewModel.VaultModel)
    {
        /// <summary>
        /// Gets the view model of the unlocked vault.
        /// </summary>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; } = unlockedVaultViewModel;
    }
}
