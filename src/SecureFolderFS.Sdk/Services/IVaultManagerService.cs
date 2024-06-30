using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
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
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the master key used to decrypt the vault.</returns>
        Task<IDisposable> CreateAsync(IFolder vaultFolder, IKey passkey, VaultOptions vaultOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks the specified <paramref name="vaultFolder"/> using the provided <paramref name="passkey"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="passkey"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the master key used to decrypt the vault.</returns>
        Task<IDisposable> UnlockAsync(IFolder vaultFolder, IKey passkey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recovers the specified <paramref name="vaultFolder"/> using the provided <paramref name="encodedMasterKey"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="encodedMasterKey">The Base64 encoded master key.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the master key used to decrypt the vault.</returns>
        Task<IDisposable> RecoverAsync(IFolder vaultFolder, string encodedMasterKey, CancellationToken cancellationToken = default);

        Task<IFolder> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken);

        Task<IFolder> CreateLocalStorageAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken);
    }
}
