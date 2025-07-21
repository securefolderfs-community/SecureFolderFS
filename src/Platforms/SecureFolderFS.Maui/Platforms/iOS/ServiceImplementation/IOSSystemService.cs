using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class IOSSystemService : ISystemService
    {
        /// <inheritdoc/>
        public event EventHandler? DeviceLocked;

        /// <inheritdoc/>
        public async Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
#if IOS
            await Task.CompletedTask;
            var path = storageRoot.Id;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Invalid storage root path.", nameof(storageRoot));

            var fileManager = NSFileManager.DefaultManager;
            var attributes = fileManager.GetFileSystemAttributes(path, out var error);
            if (attributes is null || error is not null)
                throw new IOException($"Unable to get file system attributes for path: {path}. Error: {error.LocalizedDescription}.");

            return (long)attributes.FreeSize;
#else
            await Task.CompletedTask;
            throw new PlatformNotSupportedException("Only implemented on iOS.");
#endif
        }
    }
}
