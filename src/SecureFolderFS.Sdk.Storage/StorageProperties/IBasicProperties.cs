using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Exposes access to basic storage object properties.
    /// </summary>
    public interface IBasicProperties
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

        /// <summary>
        /// Gets all available properties of the storage object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IStorageProperty{T}"/> of type <see cref="object"/> of available properties.</returns>
        IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync(CancellationToken cancellationToken = default);
    }
}
