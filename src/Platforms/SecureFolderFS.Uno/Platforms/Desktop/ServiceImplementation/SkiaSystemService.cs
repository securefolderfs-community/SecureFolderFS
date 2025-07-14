using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class SkiaSystemService : ISystemService
    {
        private EventHandler? _deviceLocked;

        /// <inheritdoc/>
        public event EventHandler? DeviceLocked // TODO: Implement on Linux and MacOS
        {
            add => _deviceLocked += value;
            remove => _deviceLocked -= value;
        }

        /// <inheritdoc/>
        public Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
            // TODO: Implement size calculation
            return Task.FromResult<long>(0);
        }
    }
}
