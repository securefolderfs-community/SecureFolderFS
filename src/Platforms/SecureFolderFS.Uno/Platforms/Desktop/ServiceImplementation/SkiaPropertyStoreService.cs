#if !WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="IPropertyStoreService"/>
    internal sealed class SkiaPropertyStoreService : BasePropertyStoreService
    {
        public SkiaPropertyStoreService(IModifiableFolder settingsFolder)
        {
            SecurePropertyStore = CreateSecureStore(settingsFolder);
        }

        /// <inheritdoc/>
        public override IPropertyStore<string> SecurePropertyStore { get; }

        /// <summary>
        /// Picks the strongest secret store available on this machine.
        /// </summary>
        private static IPropertyStore<string> CreateSecureStore(IModifiableFolder settingsFolder)
        {
#if __UNO_SKIA_MACOS__
            if (MacOsKeychainPropertyStore.IsSupported())
                return new MacOsKeychainPropertyStore();
#endif

#if !__UNO_SKIA_MACOS__ && !WINDOWS
            if (LibSecretPropertyStore.IsSupported())
                return new LibSecretPropertyStore();
#endif

            return new FallbackFilePropertyStore(settingsFolder);
        }
    }

    /// <summary>
    /// Last-resort <see cref="IPropertyStore{TKey}"/> for systems without an OS secret store.
    /// Values are persisted as JSON in a file, whose unix permissions are restricted to the owner
    /// (0600). This protects against other local users but NOT against malware running as the
    /// same user; platforms with a keyring never use this store.
    /// </summary>
    internal sealed class FallbackFilePropertyStore : IPropertyStore<string>
    {
        private const string FileName = "secure_properties.dat";

        private readonly IModifiableFolder _settingsFolder;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private Dictionary<string, string>? _cache;

        public FallbackFilePropertyStore(IModifiableFolder settingsFolder)
        {
            _settingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        public async Task<TValue?> GetValueAsync<TValue>(string key, Func<TValue?>? defaultValue = null, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var cache = await LoadAsync(cancellationToken);
                if (!cache.TryGetValue(key, out var raw))
                    return defaultValue is not null ? defaultValue() : default;

                if (typeof(TValue) == typeof(string))
                    return (TValue?)(object)raw;

                return JsonSerializer.Deserialize<TValue>(raw);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetValueAsync<TValue>(string key, TValue? value, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var cache = await LoadAsync(cancellationToken);
                cache[key] = value is string str ? str : JsonSerializer.Serialize(value);
                await PersistAsync(cache, cancellationToken);

                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var cache = await LoadAsync(cancellationToken);
                if (!cache.Remove(key))
                    return false;

                await PersistAsync(cache, cancellationToken);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task WipeAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                _cache = new Dictionary<string, string>();
                await PersistAsync(_cache, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<Dictionary<string, string>> LoadAsync(CancellationToken cancellationToken)
        {
            if (_cache is not null)
                return _cache;

            var file = await _settingsFolder.TryGetFileByNameAsync(FileName, cancellationToken);
            if (file is null)
                return _cache = new Dictionary<string, string>();

            try
            {
                var bytes = await file.ReadBytesAsync(cancellationToken);
                _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetString(bytes));
            }
            catch (JsonException)
            {
                _cache = null;
            }

            return _cache ??= new Dictionary<string, string>();
        }

        private async Task PersistAsync(Dictionary<string, string> cache, CancellationToken cancellationToken)
        {
            var file = await _settingsFolder.CreateFileAsync(FileName, overwrite: false, cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cache));
            await file.WriteBytesAsync(bytes, cancellationToken);

            RestrictToOwner(file);
        }

        private static void RestrictToOwner(IFile file)
        {
            // Best effort: on locatable (filesystem) storage the storable Id is the full path.
            try
            {
                if (!OperatingSystem.IsWindows() && File.Exists(file.Id))
                    File.SetUnixFileMode(file.Id, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            catch (Exception)
            {
                // Permissions are defense-in-depth here; failing to set them must not break persistence.
            }
        }
    }
}
#endif
