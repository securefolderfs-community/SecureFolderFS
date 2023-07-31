using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Vault
{
    /// <summary>
    /// Represents the view model of an unlocked vault.
    /// </summary>
    public sealed class UnlockedVaultViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the <see cref="Sdk.ViewModels.Vault.VaultViewModel"/> associated with this view model.
        /// </summary>
        public VaultViewModel VaultViewModel { get; }

        /// <summary>
        /// Gets the unlocked vault model that manages the life time of the vault.
        /// </summary>
        public IVaultLifetimeModel UnlockedVaultModel { get; }

        public UnlockedVaultViewModel(VaultViewModel vaultViewModel, IVaultLifetimeModel unlockedVaultModel)
        {
            VaultViewModel = vaultViewModel;
            UnlockedVaultModel = unlockedVaultModel;
        }
    }
}
