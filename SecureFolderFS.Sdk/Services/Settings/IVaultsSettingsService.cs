using System.Collections.Generic;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.Settings
{
    /// <summary>
    /// A service to manage settings of saved vaults.
    /// </summary>
    public interface IVaultsSettingsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets saved vaults by the user.
        /// </summary>
        List<VaultModel>? SavedVaults { get; set; }

        // TODO: We can add more settings in the future (vault layout, etc.)
    }
}
