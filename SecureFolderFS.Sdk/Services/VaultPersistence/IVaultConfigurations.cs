using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.Utils;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Services.VaultPersistence
{
    /// <summary>
    /// A service to manage vaults saved in SecureFolderFS.
    /// </summary>
    public interface IVaultConfigurations : IPersistable
    {
        /// <summary>
        /// Gets or sets the list of saved vaults.
        /// </summary>
        ICollection<VaultDataModel>? SavedVaults { get; set; }
    }
}
