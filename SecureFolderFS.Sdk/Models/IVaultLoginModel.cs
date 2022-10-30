using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Model 
    /// </summary>
    public interface IVaultLoginModel : IDisposable
    {
        /// <summary>
        /// Gets associated <see cref="IVaultModel"/> with this model.
        /// </summary>
        IVaultModel VaultModel { get; }

        /// <summary>
        /// An event that's fired when vault folder state has changed.
        /// </summary>
        /// <remarks>
        /// The event needs to be successfully activated through <see cref="WatchForChangesAsync"/> before it can be used.
        /// </remarks>
        event EventHandler<IResult>? VaultChangedEvent;

        /// <summary>
        /// Starts watching for any changes of the vault folder and reports it to <see cref="VaultChangedEvent"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true if the watcher has been properly set up, otherwise false.</returns>
        Task<bool> WatchForChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Locks the vault folder preventing the deletion of it.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful. returns a disposable lock handle, otherwise null.</returns>
        Task<IDisposable?> LockFolderAsync(CancellationToken cancellationToken = default);
    }
}
