using System.Threading.Tasks;
using Google.Apis.Util.Store;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.GoogleDrive
{
    internal sealed class PropertyDataStore : IDataStore
    {
        private readonly IPropertyStore<string> _propertyStore;

        public PropertyDataStore(IPropertyStore<string> propertyStore)
        {
            _propertyStore = propertyStore;
        }

        /// <inheritdoc/>
        public async Task StoreAsync<T>(string key, T value)
        {
            await _propertyStore.SetValueAsync(key, value);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync<T>(string key)
        {
            await _propertyStore.RemoveAsync(key);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key)
        {
            return await _propertyStore.GetValueAsync<T>(key);
        }

        /// <inheritdoc/>
        public async Task ClearAsync()
        {
            await _propertyStore.WipeAsync();
        }
    }
}
