using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels.Database;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IPropertyStoreService"/>
    public abstract class BasePropertyStoreService : IPropertyStoreService
    {
        /// <inheritdoc/>
        public abstract IPropertyStore<string> SecurePropertyStore { get; }
        
        /// <inheritdoc/>
        public virtual IPropertyStore<string> InMemoryPropertyStore { get; } = new InMemoryPropertyStore();

        /// <inheritdoc/>
        public virtual IDatabaseModel<string> GetDatabaseModel(IFile databaseFile)
        {
            return new SingleFileDatabaseModel(databaseFile, StreamSerializer.Instance);
        }
    }

    /// <inheritdoc cref="IPropertyStore{TKey}"/>
    public sealed class InMemoryPropertyStore : IPropertyStore<string>
    {
        private readonly Dictionary<string, object?> _properties = new();
        
        /// <inheritdoc/>
        public async Task<TValue?> GetValueAsync<TValue>(string key, Func<TValue?>? defaultValue = null, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (_properties.TryGetValue(key, out var value))
                return (TValue?)value;

            return default;
        }

        /// <inheritdoc/>
        public Task<bool> SetValueAsync<TValue>(string key, TValue? value, CancellationToken cancellationToken = default)
        {
            _properties[key] = value;
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_properties.Remove(key));
        }

        public Task WipeAsync(CancellationToken cancellationToken = default)
        {
            _properties.Clear();
            return Task.CompletedTask;
        }
    }
}
