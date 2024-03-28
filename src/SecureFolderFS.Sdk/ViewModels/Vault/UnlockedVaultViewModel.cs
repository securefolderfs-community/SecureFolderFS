using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;

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
        /// Gets the unlocked root folder of the vault.
        /// </summary>
        public IFolder StorageRoot { get; }

        public UnlockedVaultViewModel(VaultViewModel vaultViewModel, IFolder storageRoot)
        {
            VaultViewModel = vaultViewModel;
            StorageRoot = storageRoot;
        }
    }
}
