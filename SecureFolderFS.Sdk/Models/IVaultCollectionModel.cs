using SecureFolderFS.Shared.Utils;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages a collection of saved vaults.
    /// </summary>
    public interface IVaultCollectionModel : IPersistable
    {
        /// <summary>
        /// Adds provided <paramref name="vaultModel"/> to the saved vaults list.
        /// </summary>
        /// <param name="vaultModel">The vault to add.</param>
        /// <returns>If successful, returns true, otherwise false.</returns>
        bool AddVault(IVaultModel vaultModel);

        /// <summary>
        /// Removes provided <paramref name="vaultModel"/> from the saved vaults list.
        /// </summary>
        /// <param name="vaultModel">The vault to remove.</param>
        /// <returns>If successful, returns true, otherwise false.</returns>
        bool RemoveVault(IVaultModel vaultModel);

        /// <summary>
        /// Gets all persisted vault models.
        /// </summary>
        /// <returns>Returns an <see cref="IEnumerable{T}"/> of type <see cref="IVaultModel"/> of all saved vaults.</returns>
        IEnumerable<IVaultModel> GetVaults();
    }
}