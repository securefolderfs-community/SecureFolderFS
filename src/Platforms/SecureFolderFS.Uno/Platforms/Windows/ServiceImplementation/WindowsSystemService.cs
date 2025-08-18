using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
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
        public async Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
#if WINDOWS
            try
            {
                if (storageRoot is not SystemFolder systemFolder)
                    throw new InvalidOperationException($"The {nameof(storageRoot)} is not of type {nameof(SystemFolder)}.");

                var root = systemFolder.Info.Root.FullName;
                var drive = DriveInfo.GetDrives()
                    .FirstOrDefault(x => x.IsReady && x.RootDirectory.FullName.Equals(root, StringComparison.OrdinalIgnoreCase));

                if (drive is null)
                    throw new IOException($"Unable to determine the drive for path: '{storageRoot.Id}'.");

                return drive.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to get available free space for path: '{storageRoot.Id}'.", ex);
            }
#else
            throw new PlatformNotSupportedException("Only implemented on Windows.");
#endif
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
