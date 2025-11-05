using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Models
{
    public interface IVaultUnlockingModel : IDisposable
    {
        /// <summary>
        /// Gets the folder associated with the vault.
        /// </summary>
        IFolder VaultFolder { get; }

        /// <summary>
        /// Unlocks the vault using the provided credentials.
        /// </summary>
        /// <param name="credentials">The credentials required to unlock the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the unlock contract represented by <see cref="IDisposable"/>.</returns>
        Task<IDisposable> UnlockAsync(IKey credentials, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recovers the specified vault using the provided <paramref name="encodedRecoveryKey"/>.
        /// </summary>
        /// <param name="encodedRecoveryKey">The Base64 encoded recovery key.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> RecoverAsync(string encodedRecoveryKey, CancellationToken cancellationToken = default);
    }
}
