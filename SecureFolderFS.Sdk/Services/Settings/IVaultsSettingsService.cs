using System.Collections.Generic;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage vaults saved in SecureFolderFS.
    /// </summary>
    public interface IVaultsSettingsService : IPersistable
    {
        /// <summary>
        /// Gets or sets the list of saved vaults.
        /// </summary>
        IList<VaultDataModel> SavedVaults { get; set; }
    }
}
