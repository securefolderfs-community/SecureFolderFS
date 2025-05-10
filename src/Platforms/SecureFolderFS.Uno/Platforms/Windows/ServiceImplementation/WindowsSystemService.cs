using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class WindowsSystemService : ISystemService
    {
        private EventHandler? _deviceLocked;

        /// <inheritdoc/>
        public event EventHandler? DeviceLocked
        {
            add
            {
                if (_deviceLocked is null)
                    SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

                _deviceLocked += value;
            }
            remove
            {
                _deviceLocked -= value;
                if (_deviceLocked is null)
                    SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            }
        }

        /// <inheritdoc/>
        public Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            return Task.FromException<long>(new NotImplementedException());
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                    _deviceLocked?.Invoke(sender, e);
                    break;

                default: return;
            }
        }
    }
}
