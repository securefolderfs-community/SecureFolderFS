#if APP_PLATFORM_PRESENT
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.AppPlatform;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    // TODO: Testing Purposes Only! Store the device key securely later
    
    /// <summary>
    /// File-based <see cref="IDeviceKeyStore"/> for desktop platforms.
    /// Stores the device private key and device ID in the application directory.
    /// </summary>
    internal sealed class FileDeviceKeyStore : IDeviceKeyStore
    {
        private const string DeviceKeyFileName = ".appplatform-device-key";
        private const string DeviceIdFileName = ".appplatform-device-id";

        private readonly string _basePath;

        public FileDeviceKeyStore(string basePath)
        {
            _basePath = basePath;
        }

        public Task<bool> HasPrivateKeyAsync(CancellationToken ct = default)
        {
            return Task.FromResult(File.Exists(Path.Combine(_basePath, DeviceKeyFileName)));
        }

        public Task<byte[]> GetPrivateKeyAsync(CancellationToken ct = default)
        {
            var path = Path.Combine(_basePath, DeviceKeyFileName);
            if (!File.Exists(path))
                throw new InvalidOperationException("No device key stored. Complete App Platform setup first.");

            return File.ReadAllBytesAsync(path, ct);
        }

        public async Task StorePrivateKeyAsync(byte[] privateKey, CancellationToken ct = default)
        {
            var path = Path.Combine(_basePath, DeviceKeyFileName);
            await File.WriteAllBytesAsync(path, privateKey, ct);
        }

        public Task<Guid?> GetDeviceIdAsync(CancellationToken ct = default)
        {
            var path = Path.Combine(_basePath, DeviceIdFileName);
            if (!File.Exists(path))
                return Task.FromResult<Guid?>(null);

            var text = File.ReadAllText(path);
            return Task.FromResult<Guid?>(Guid.TryParse(text, out var id) ? id : null);
        }

        public async Task StoreDeviceIdAsync(Guid deviceId, CancellationToken ct = default)
        {
            var path = Path.Combine(_basePath, DeviceIdFileName);
            await File.WriteAllTextAsync(path, deviceId.ToString(), ct);
        }

        public Task ClearAsync(CancellationToken ct = default)
        {
            var keyPath = Path.Combine(_basePath, DeviceKeyFileName);
            var idPath = Path.Combine(_basePath, DeviceIdFileName);

            if (File.Exists(keyPath)) File.Delete(keyPath);
            if (File.Exists(idPath)) File.Delete(idPath);

            return Task.CompletedTask;
        }
    }
}
#endif
