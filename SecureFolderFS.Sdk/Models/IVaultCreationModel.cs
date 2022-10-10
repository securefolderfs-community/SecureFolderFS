using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model used for creating new vaults.
    /// </summary>
    public interface IVaultCreationModel : IDisposable
    {
        /// <summary>
        /// Sets the vault folder and initializes it with data.
        /// </summary>
        /// <param name="folder">The vault folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of action.</returns>
        Task<IResult> SetFolderAsync(IModifiableFolder folder, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the <see cref="IKeystoreModel"/> to use for vault creation.
        /// </summary>
        /// <param name="keystoreModel">The keystore to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that represents the action.</returns>
        Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the password used for the vault.
        /// </summary>
        /// <param name="password">An instance of <see cref="IPassword"/> to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true, otherwise false.</returns>
        Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the cipher scheme to be used for encryption.
        /// </summary>
        /// <param name="contentCipher">The content cipher scheme to use.</param>
        /// <param name="nameCipher">The filename cipher scheme to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the cipher scheme is supported and was properly set, returns true, otherwise false.</returns>
        Task<bool> SetCipherSchemeAsync(ICipherInfoModel contentCipher, ICipherInfoModel nameCipher, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finalizes the creation routine.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult{T}"/> of <see cref="IVaultModel"/> that represents the action.</returns>
        Task<IResult<IVaultModel?>> DeployAsync(CancellationToken cancellationToken = default);
    }
}
