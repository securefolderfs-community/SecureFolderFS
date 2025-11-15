using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IPropertyStoreService"/>
    internal sealed class MauiPropertyStoreService : BasePropertyStoreService
    {
        /// <inheritdoc/>
        public override IPropertyStore<string> SecurePropertyStore { get; } = new SecureStoragePropertyStore(StreamSerializer.Instance);
        //public override IPropertyStore<string> SecurePropertyStore { get; } = new InMemoryPropertyStore();
    }

    /// <inheritdoc cref="IPropertyStore{TKey}"/>
    file sealed class SecureStoragePropertyStore : IPropertyStore<string>
    {
        private readonly IAsyncSerializer<Stream> _serializer;

        public SecureStoragePropertyStore(IAsyncSerializer<Stream> serializer)
        {
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public async Task<TValue?> GetValueAsync<TValue>(string key, Func<TValue?>? defaultValue = null, CancellationToken cancellationToken = default)
        {
            var rawString = await SecureStorage.Default.GetAsync(key);
            if (string.IsNullOrEmpty(rawString))
                return defaultValue is not null ? defaultValue.Invoke() : default;
            
            var deserialized = await _serializer.TryDeserializeFromStringAsync<TValue>(rawString, cancellationToken);
            return deserialized is not null ? deserialized : (defaultValue is not null ? defaultValue.Invoke() : default);
        }

        /// <inheritdoc/>
        public async Task<bool> SetValueAsync<TValue>(string key, TValue? value, CancellationToken cancellationToken = default)
        {
            var serialized = await _serializer.TrySerializeToStringAsync(value, cancellationToken);
            await SecureStorage.Default.SetAsync(key, serialized ?? string.Empty);
            
            return true;
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SecureStorage.Default.Remove(key));
        }

        /// <inheritdoc/>
        public Task WipeAsync(CancellationToken cancellationToken = default)
        {
            SecureStorage.Default.RemoveAll();
            return Task.CompletedTask;
        }
    }
}
