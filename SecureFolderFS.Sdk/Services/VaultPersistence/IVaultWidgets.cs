using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.Utils;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Services.VaultPersistence
{
    /// <summary>
    /// A service to manage widgets of saved vaults.
    /// </summary>
    public interface IVaultWidgets : IPersistable
    {
        /// <summary>
        /// Gets the collection of widgets saved for an individual vault.
        /// </summary>
        /// <param name="id">The ID that is associated with a vault.</param>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="WidgetDataModel"/> that represents the vault widgets.</returns>
        ICollection<WidgetDataModel>? GetForVault(string id);

        /// <summary>
        /// Sets the collection of widgets for an individual vault.
        /// </summary>
        /// <param name="id">The ID that is associated with a vault.</param>
        /// <param name="widgets">The collection of widgets to set.</param>
        /// <returns>If successful, returns true, otherwise false.</returns>
        bool SetForVault(string id, ICollection<WidgetDataModel>? widgets);

        /// <summary>
        /// Clears all persisted widgets.
        /// </summary>
        void Clear();
    }
}
