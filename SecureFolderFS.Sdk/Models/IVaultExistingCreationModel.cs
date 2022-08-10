using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model used for adding existing vaults.
    /// </summary>
    public interface IVaultExistingCreationModel
    {
        /// <summary>
        /// Sets the vault folder and retrieves configuration from it.
        /// </summary>
        /// <param name="folder">The vault folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of action.</returns>
        Task<IResult> SetFolderAsync(IFolder folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finalizes the routine and creates <see cref="IVaultModel"/> for the vault.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task<IResult<IVaultModel?>> DeployAsync(CancellationToken cancellationToken = default);
    }
}
