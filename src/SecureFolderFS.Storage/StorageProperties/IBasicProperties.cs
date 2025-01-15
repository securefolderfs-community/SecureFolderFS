using OwlCore.Storage;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Storage.StorageProperties
{
    /// <summary>
    /// Exposes access to basic storage object properties.
    /// </summary>
    public interface IBasicProperties
    {
        /// <summary>
        /// Gets all available properties of the storage object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IStorageProperty{T}"/> of type <see cref="object"/> of available properties.</returns>
        IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync(CancellationToken cancellationToken = default);
    }
}
