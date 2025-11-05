using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// Represents a model responsible for migrating vaults to a new format.
    /// </summary>
    public interface IVaultMigratorModel : IVaultUnlockingModel
    {
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
