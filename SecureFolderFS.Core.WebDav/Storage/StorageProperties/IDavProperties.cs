using SecureFolderFS.Core.WebDav.Http.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage.StorageProperties
{
    /// <summary>
    /// Extends <see cref="IBasicProperties"/> with common WebDav item properties.
    /// </summary>
    internal interface IDavProperties : IBasicProperties
    {
        /// <summary>
        /// Gets the value of ETag header value.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <param name="skipExpensive">Determines whether to avoid an expensive call to retrieve this property.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IStorageProperty{T}"/> of type <see cref="ulong"/> that represents the content language.</returns>
        Task<IStorageProperty<string>?> GetEtagAsync(bool skipExpensive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the value of Content-Type header value.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <param name="skipExpensive">Determines whether to avoid an expensive call to retrieve this property.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IStorageProperty{T}"/> of type <see cref="ulong"/> that represents the content type.</returns>
        Task<IStorageProperty<string>?> GetContentTypeAsync(bool skipExpensive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the value of Content-Language header value.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <param name="skipExpensive">Determines whether to avoid an expensive call to retrieve this property.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IStorageProperty{T}"/> of type <see cref="ulong"/> that represents the content language.</returns>
        Task<IStorageProperty<string>?> GetContentLanguageAsync(bool skipExpensive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the size taken by the storage object on the drive, in bytes. Value is null, if the size cannot be fetched.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <param name="skipExpensive">Determines whether to avoid an expensive call to retrieve this property.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IStorageProperty{T}"/> of type <see cref="ulong"/> that represents the size.</returns>
        Task<IStorageProperty<ulong?>?> GetSizeAsync(bool skipExpensive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a WebDav property identified by <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <see cref="IStorageProperty{T}"/>.</returns>
        Task<IStorageProperty<string>> GetPropertyAsync(string propertyName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all identifiers which identify properties of the storage object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="DavPropertyIdentifier"/> for properties.</returns>
        IAsyncEnumerable<DavPropertyIdentifier> GetIdentifiersAsync(CancellationToken cancellationToken = default);
    }
}
