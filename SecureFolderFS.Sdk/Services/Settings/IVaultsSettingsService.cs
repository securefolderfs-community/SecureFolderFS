using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage settings of saved vaults.
    /// </summary>
    public interface IVaultsSettingsService : IPersistable
    {
        /// <summary>
        /// Gets vault context identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id associated with a context.</param>
        VaultContextDataModel GetVaultContextForId(string id);
    }
}
