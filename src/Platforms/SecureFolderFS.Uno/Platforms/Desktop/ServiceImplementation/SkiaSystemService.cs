using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed partial class SkiaSystemService : ISystemService
    {
        private EventHandler? _deviceLocked;
        
#if !__UNO_SKIA_MACOS__
        /// <inheritdoc/>
        public event EventHandler? DeviceLocked
        {
            add => _deviceLocked += value;
            remove => _deviceLocked -= value;
        }
#endif

        /// <inheritdoc/>
        public Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
            try
            {
                var drive = new DriveInfo("/");
                if (!drive.IsReady)
                    return Task.FromResult(0L);

                return Task.FromResult(drive.AvailableFreeSpace);
            }
            catch (Exception)
            {
                return Task.FromResult(0L);
            }
        }
    }
}
