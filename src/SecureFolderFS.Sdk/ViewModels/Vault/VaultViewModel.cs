using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Vault
{
    /// <summary>
    /// Represents the view model of a vault.
    /// </summary>
    public sealed class VaultViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the vault model associated with the vault.
        /// </summary>
        public IVaultModel VaultModel { get; }

        /// <summary>
        /// Gets the context model that represents all widgets in this vault.
        /// </summary>
        public IWidgetsCollectionModel WidgetsContextModel { get; }

        public VaultViewModel(IVaultModel vaultModel, IWidgetsCollectionModel widgetsContextModel)
        {
            VaultModel = vaultModel;
            WidgetsContextModel = widgetsContextModel;
        }
    }
}
