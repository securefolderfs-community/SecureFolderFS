using NWebDav.Server.Storage;
using NWebDav.Server.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    internal sealed class DavStorageProperties<TStorable> : IDavProperties
        where TStorable : IDavStorable
    {
        /// <summary>
        /// Gets or sets the storable object of which properties to get.
        /// </summary>
        public TStorable? Storable { get; set; }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<string>?> GetEtagAsync(bool skipExpensive = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<string>?> GetContentTypeAsync(bool skipExpensive = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<string>?> GetContentLanguageAsync(bool skipExpensive = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<ulong?>?> GetSizeAsync(bool skipExpensive = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<string>> GetPropertyAsync(string propertyName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<DavPropertyIdentifier> GetIdentifiersAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
