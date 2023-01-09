using NWebDav.Server.Storage;
using NWebDav.Server.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    internal sealed class DavBasicProperties<TStorable> : IDavProperties
        where TStorable : IDavStorable
    {
        // Justification: Storable is late-initialized
#pragma warning disable CS8618
        public TStorable? Storable { get; set; }
#pragma warning restore CS8618

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
