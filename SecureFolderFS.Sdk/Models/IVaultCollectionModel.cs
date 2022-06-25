using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages a collection of saved vaults.
    /// </summary>
    public interface IVaultCollectionModel
    {
        /// <summary>
        /// Adds provided <see cref="vault"/> to the saved vaults list.
        /// </summary>
        /// <param name="vault">The vault to add.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task AddVaultAsync(IVaultModel vault);
        
        /// <summary>
        /// Removes provided <see cref="vault"/> from the saved vaults list.
        /// </summary>
        /// <param name="vault">The vault to remove.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RemoveVaultAsync(IVaultModel vault);

        /// <summary>
        /// Initializes and gets all saved vaults.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IVaultModel"/> of saved vaults.</returns>
        IAsyncEnumerable<IVaultModel> GetVaultsAsync(CancellationToken cancellationToken = default);
    }
}