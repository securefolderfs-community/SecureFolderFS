using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage all saved vaults by the user.
    /// </summary>
    public interface ISavedVaultsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets saved vaults by the user.
        /// </summary>
        List<string>? VaultPaths { get; set; }

        // TODO: We can add more settings in the future (vault layout, etc.)
    }
}
