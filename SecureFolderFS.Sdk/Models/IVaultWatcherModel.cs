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
    public interface IVaultWatcherModel : IAsyncInitialize, INotifyStateChanged, IDisposable
    {
        /// <summary>
        /// Gets the vault folder that's being watched.
        /// </summary>
        IFolder VaultFolder { get; }

        /// <summary>
        /// Locks the vault folder preventing the deletion of it.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns a lock handle represented by <see cref="IAsyncDisposable"/>, otherwise null.</returns>
        Task<IDisposable?> LockFolderAsync(CancellationToken cancellationToken = default);
    }
}
