using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model used for unlocking vaults.
    /// </summary>
    public interface IVaultUnlockingModel : IDisposable
    {
        /// <summary>
        /// Sets the <see cref="IFolder"/> that represents the vault and retrieves the configuration file.
        /// </summary>
        /// <param name="folder">The vault folder to be set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the folder was successfully set and configuration file found, returns true, otherwise false.</returns>
        Task<IResult> SetFolderAsync(IFolder folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the <see cref="IKeystoreModel"/> associated with the vault.
        /// </summary>
        /// <param name="keystoreModel">The keystore to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that determines whether the keystore was set successfully.</returns>
        Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks the vault using provided <paramref name="password"/>.
        /// </summary>
        /// <param name="password">The password used for unlocking the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult{T}"/> of type <see cref="IUnlockedVaultModel"/> that represents the action.</returns>
        Task<IResult<IUnlockedVaultModel?>> UnlockAsync(IPassword password, CancellationToken cancellationToken = default);
    }
}
