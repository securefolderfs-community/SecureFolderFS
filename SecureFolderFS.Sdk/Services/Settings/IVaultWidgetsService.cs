using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage widgets of saved vaults.
    /// </summary>
    public interface IVaultWidgetsService : IPersistable
    {
        /// <summary>
        /// Gets widget context identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id associated with a context.</param>
        WidgetsContextDataModel GetWidgetsContextForId(string id);
    }
}
