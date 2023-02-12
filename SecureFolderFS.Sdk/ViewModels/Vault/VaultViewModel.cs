using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Vault
{
    /// <summary>
    /// Represents the view model of a vault.
    /// </summary>
    public sealed class VaultViewModel : ObservableObject, IEquatable<VaultViewModel>
    {
        /// <summary>
        /// Gets the vault model associated with the vault.
        /// </summary>
        public IVaultModel VaultModel { get; }

        /// <summary>
        /// Gets the context model that represents all widgets in this vault.
        /// </summary>
        public IWidgetsContextModel WidgetsContextModel { get; }

        public VaultViewModel(IVaultModel vaultModel, IWidgetsContextModel widgetsContextModel)
        {
            VaultModel = vaultModel;
            WidgetsContextModel = widgetsContextModel;
        }

        /// <inheritdoc/>
        public bool Equals(VaultViewModel? other)
        {
            if (other is null)
                return false;

            return VaultModel.Equals(other.VaultModel);
        }
    }
}
