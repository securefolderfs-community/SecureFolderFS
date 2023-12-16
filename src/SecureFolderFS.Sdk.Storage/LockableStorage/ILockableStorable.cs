using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.LockableStorage
{
    /// <summary>
    /// Represents a storage object that can be locked preventing file system access or deletion of it.
    /// </summary>
    public interface ILockableStorable : IStorable
    {
        /// <summary>
        /// Tries to obtain an exclusive lock to the storage object to prevent the deletion of it.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns a lock handle to the storage object represented by <see cref="IAsyncDisposable"/>; otherwise null.</returns>
        Task<IDisposable?> ObtainLockAsync(CancellationToken cancellationToken = default);
    }
}
