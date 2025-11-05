using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultManagerService
    {
        /// <summary>
        /// Creates new or overwrites an existing vault in the specified <paramref name="vaultFolder"/>.
        /// </summary>
        /// <remarks>
        /// To retrieve the decryption key, call the <c>.ToString()</c> method
        /// on the returned <see cref="IDisposable"/> instance.
        /// Since the key returned by this method can be used to decrypt vault contents
        /// regardless of whether the vault passkey was changed, it is, by nature,
        /// very sensitive and should be disposed of as soon as it is no longer needed.
        /// </remarks>
        /// <param name="vaultFolder">The folder where the vault should be created.</param>
        /// <param name="passkey">The passkey represented by <see cref="IEnumerable{T}"/> of <see cref="IDisposable"/> representing authentication elements to set for this vault.</param>
        /// <param name="vaultOptions">The required options to set for this vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> CreateAsync(IFolder vaultFolder, IKey passkey, VaultOptions vaultOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks the specified <paramref name="vaultFolder"/> using the provided <paramref name="passkey"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="passkey"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> UnlockAsync(IFolder vaultFolder, IKey passkey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recovers the specified <paramref name="vaultFolder"/> using the provided <paramref name="encodedRecoveryKey"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="encodedRecoveryKey">The Base64 encoded recovery key.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> RecoverAsync(IFolder vaultFolder, string encodedRecoveryKey, CancellationToken cancellationToken = default);

        // TODO: Consider using IVaultUnlockingModel
        //Task<IVaultUnlockingModel> GetUnlockingModelAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Modifies the configured authentication for the specified <paramref name="vaultFolder"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="unlockContract">The recovery key used to decrypt the vault</param>
        /// <param name="newPasskey">The new passkey to secure the vault with.</param>
        /// <param name="vaultOptions">>The required options to set for this vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ModifyAuthenticationAsync(IFolder vaultFolder, IDisposable unlockContract, IKey newPasskey, VaultOptions vaultOptions, CancellationToken cancellationToken = default);
    }
}
