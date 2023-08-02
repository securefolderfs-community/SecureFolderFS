using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Represents a service to manage vault-related data.
    /// </summary>
    public interface IVaultPersistenceService : IPersistable
    {
        /// <summary>
        /// Gets the service that contains widget data of saved vaults
        /// </summary>
        IVaultWidgets VaultWidgets { get; }

        /// <summary>
        /// Gets the service that contains configurations of saved vaults.
        /// </summary>
        IVaultConfigurations VaultConfigurations { get; }
    }
}
