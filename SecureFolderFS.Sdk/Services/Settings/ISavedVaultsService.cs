using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage all saved vaults by the user.
    /// </summary>
    public interface ISavedVaultsService : IPersistable
    {
        /// <summary>
        /// Gets or sets saved vaults by the user.
        /// </summary>
        List<string>? VaultPaths { get; set; }
    }
}
