using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Services.VaultPersistence
{
    /// <summary>
    /// A service to manage widgets of saved vaults.
    /// </summary>
    public interface IVaultWidgets : IPersistable
    {
        /// <summary>
        /// Sets the widgets data for specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique ID of a vault.</param>
        /// <param name="widgetDataModel">The widget data to set. If null, discards the saved data associated with <paramref name="id"/>.</param>
        void SetWidgetsData(string id, WidgetDataModel? widgetDataModel);

        /// <summary>
        /// Gets the widgets data for specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique ID of a vault.</param>
        /// <returns>If the widget data was present, returns <see cref="WidgetsCollectionDataModel"/>, otherwise null.</returns>
        WidgetsCollectionDataModel? GetWidgetsData(string id);
    }
}
