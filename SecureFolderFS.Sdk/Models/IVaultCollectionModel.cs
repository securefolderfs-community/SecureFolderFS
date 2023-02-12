using SecureFolderFS.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages a collection of saved vaults.
    /// </summary>
    public interface IVaultCollectionModel : IAsyncInitialize
    {
        /// <summary>
        /// Gets the value indicating whether there are any vaults added.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Adds provided <see cref="vaultModel"/> to the saved vaults list.
        /// </summary>
        /// <param name="vaultModel">The vault to add.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> AddVaultAsync(IVaultModel vaultModel, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Removes provided <see cref="vaultModel"/> from the saved vaults list.
        /// </summary>
        /// <param name="vaultModel">The vault to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> RemoveVaultAsync(IVaultModel vaultModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes and gets all saved vaults.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IVaultModel"/> of saved vaults.</returns>
        IAsyncEnumerable<IVaultModel> GetVaultsAsync(CancellationToken cancellationToken = default);
    }
}