using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Vault
{
    /// <summary>
    /// Represents the view model of an unlocked vault.
    /// </summary>
    public sealed class VaultViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the unlocked vault model that manages the life time of the vault.
        /// </summary>
        public IUnlockedVaultModel UnlockedVaultModel { get; }

        /// <summary>
        /// Gets the context model that represents all widgets in this vault.
        /// </summary>
        public IWidgetsContextModel WidgetsContextModel { get; }

        /// <summary>
        /// Gets the vault model associated with the vault.
        /// </summary>
        public IVaultModel VaultModel { get; }

        public VaultViewModel(IUnlockedVaultModel unlockedVaultModel, IWidgetsContextModel widgetsContextModel, IVaultModel vaultModel)
        {
            UnlockedVaultModel = unlockedVaultModel;
            WidgetsContextModel = widgetsContextModel;
            VaultModel = vaultModel;
        }
    }
}
