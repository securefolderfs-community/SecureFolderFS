using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Storage.StorageProperties
{
    /// <summary>
    /// Exposes access to storage object size properties.
    /// </summary>
    public interface ISizeProperties
    {
        /// <summary>
        /// Gets the size of the storage object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IStorageProperty{T}"/> of type <see cref="long"/> that represents the size.</returns>
        Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default);
    }
}