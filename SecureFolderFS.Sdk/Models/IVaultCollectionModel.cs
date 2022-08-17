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
        /// Checks whether there are any vaults saved.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true if the store contains vaults, otherwise false.</returns>
        Task<bool> HasVaultsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds provided <see cref="vault"/> to the saved vaults list.
        /// </summary>
        /// <param name="vault">The vault to add.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> AddVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Removes provided <see cref="vault"/> from the saved vaults list.
        /// </summary>
        /// <param name="vault">The vault to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> RemoveVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes and gets all saved vaults.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IVaultModel"/> of saved vaults.</returns>
        IAsyncEnumerable<IVaultModel> GetVaultsAsync(CancellationToken cancellationToken = default);
    }
}