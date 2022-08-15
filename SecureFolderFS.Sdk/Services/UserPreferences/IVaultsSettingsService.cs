using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage settings of saved vaults.
    /// </summary>
    public interface IVaultsSettingsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets the vault contexts associated with each vault.
        /// </summary>
        VaultContextDataModel GetVaultContextForId(string id);
    }
}
