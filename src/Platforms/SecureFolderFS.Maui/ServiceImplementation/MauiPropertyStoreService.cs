using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IPropertyStoreService"/>
    internal sealed class MauiPropertyStoreService : IPropertyStoreService
    {
        /// <inheritdoc/>
        public IPropertyStore<string> SecurePropertyStore { get; } = new SecureStoragePropertyStore();
    }

    /// <inheritdoc cref="IPropertyStore{TKey}"/>
    file sealed class SecureStoragePropertyStore : IPropertyStore<string>
    {
        /// <inheritdoc/>
        public async Task<TValue?> GetValueAsync<TValue>(string key, Func<TValue?>? defaultValue = null, CancellationToken cancellationToken = default)
        {
            if (typeof(TValue) != typeof(string))
                throw new ArgumentException("Value type must be string.", nameof(TValue));

            return (TValue?)(object?)await SecureStorage.Default.GetAsync(key);
        }

        /// <inheritdoc/>
        public async Task<bool> SetValueAsync<TValue>(string key, TValue? value, CancellationToken cancellationToken = default)
        {
            if (typeof(TValue) != typeof(string))
                throw new ArgumentException("Value type must be string.", nameof(TValue));

            if (value is not string strValue)
                return false;

            await SecureStorage.Default.SetAsync(key, strValue);
            return true;
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SecureStorage.Default.Remove(key));
        }
    }
}
