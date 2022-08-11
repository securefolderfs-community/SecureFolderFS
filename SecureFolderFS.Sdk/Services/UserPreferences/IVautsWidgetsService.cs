using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage widgets of saved vaults.
    /// </summary>
    public interface IVautsWidgetsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets the widgets contexts associated with each vault.
        /// </summary>
        WidgetsContextDataModel GetWidgetsContextForId(string id);
    }
}
