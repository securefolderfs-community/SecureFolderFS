using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model used for monitoring vault contents. 
    /// </summary>
    public interface IVaultWatcherModel : IAsyncInitialize, IDisposable
    {
        /// <summary>
        /// Gets the vault folder that's being watched.
        /// </summary>
        IFolder VaultFolder { get; }

        /// <summary>
        /// An event that's fired when integral parts of vault folder are changed.
        /// The state of vault contents is determined by <see cref="IResult.Successful"/>.
        /// If the value is true, the state of vault contents has not been changed, otherwise the validity of the vault should be reevaluated.
        /// </summary>
        /// <remarks>
        /// This event handler needs to be activated through <see cref="IVaultWatcherModel.InitAsync"/> before it can receive any events.
        /// </remarks>
        event EventHandler<IResult>? VaultChangedEvent;

        /// <summary>
        /// Locks the vault folder preventing the deletion of it.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns a lock handle represented by <see cref="IAsyncDisposable"/>, otherwise null.</returns>
        Task<IDisposable?> LockFolderAsync(CancellationToken cancellationToken = default);
    }
}
