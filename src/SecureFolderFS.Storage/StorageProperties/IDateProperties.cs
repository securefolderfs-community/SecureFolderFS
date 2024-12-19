using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.StorageProperties
{
    /// <summary>
    /// Exposes access to date-related storage object properties.
    /// </summary>
    public interface IDateProperties
    {
        /// <summary>
        /// Gets the date created of the storage object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IStorageProperty{T}"/> of type <see cref="DateTime"/> that represents date created.</returns>
        Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the date modified of the storage object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IStorageProperty{T}"/> of type <see cref="DateTime"/> that represents date modified.</returns>
        Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default);
    }
}
