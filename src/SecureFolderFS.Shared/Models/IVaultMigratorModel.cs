using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// Represents a model responsible for migrating vaults to a new format.
    /// </summary>
    public interface IVaultMigratorModel : IDisposable
    {
        /// <summary>
        /// Gets the folder associated with the vault.
        /// </summary>
        IFolder VaultFolder { get; }

        /// <summary>
        /// Unlocks the vault using the provided credentials.
        /// </summary>
        /// <typeparam name="T">The type of the credentials used to unlock the vault.</typeparam>
        /// <param name="credentials">The credentials required to unlock the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the unlock contract represented by <see cref="IDisposable"/>.</returns>
        Task<IDisposable> UnlockAsync<T>(T credentials, CancellationToken cancellationToken = default);

        /// <summary>
        /// Migrates the vault to a new format or structure.
        /// </summary>
        /// <param name="unlockContract">The disposable contract used to unlock the vault.</param>
        /// <param name="progress">The progress model to report migration progress.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task MigrateAsync(IDisposable unlockContract, ProgressModel<IResult> progress, CancellationToken cancellationToken = default);
    }
}
