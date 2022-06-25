using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Exposes access to properties of a storage object.
    /// </summary>
    public interface IStoragePropertiesCollection
    {
        /// <summary>
        /// Gets the date created of the storage object.
        /// </summary>
        DateTime DateCreated { get; }

        /// <summary>
        /// Gets the date modified of the storage object.
        /// </summary>
        DateTime DateModified { get; }

        /// <summary>
        /// Gets the size taken by the storage object on the drive, in bytes. The value is null, if the size cannot be fetched.
        /// </summary>
        ulong? Size { get; }

        /// <summary>
        /// Requests and reads all properties of an storage object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is successful, returns <see cref="IEnumerable{T}"/> of type <see cref="IStorageProperty"/>, otherwise null.</returns>
        Task<IEnumerable<IStorageProperty>?> GetStoragePropertiesAsync(CancellationToken cancellationToken = default);
    }
}
