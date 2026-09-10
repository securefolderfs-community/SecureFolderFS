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
        private const string STARTUP_TASK_ID = "SecureFolderFSStartupTask";
        private const string AUTOSTART_REGISTRY_SUBKEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AUTOSTART_ENTRY_NAME = "SecureFolderFS";

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

        /// <inheritdoc/>
        public Task<bool> IsAutoStartEnabledAsync(CancellationToken cancellationToken = default)
        {
#if UNPACKAGED
            try
            {
                using var runKey = Registry.CurrentUser.OpenSubKey(AUTOSTART_REGISTRY_SUBKEY);
                return Task.FromResult(runKey?.GetValue(AUTOSTART_ENTRY_NAME) is not null);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
#elif WINDOWS
            return GetStartupTaskStateAsync();

            static async Task<bool> GetStartupTaskStateAsync()
            {
                var startupTask = await global::Windows.ApplicationModel.StartupTask.GetAsync(STARTUP_TASK_ID);
                return startupTask.State
                    is global::Windows.ApplicationModel.StartupTaskState.Enabled
                    or global::Windows.ApplicationModel.StartupTaskState.EnabledByPolicy;
            }
#else
            throw new PlatformNotSupportedException("Only implemented on Windows.");
#endif
        }

        /// <inheritdoc/>
        public Task<bool> TrySetAutoStartAsync(bool isEnabled, CancellationToken cancellationToken = default)
        {
#if UNPACKAGED
            try
            {
                using var runKey = Registry.CurrentUser.CreateSubKey(AUTOSTART_REGISTRY_SUBKEY);
                if (isEnabled)
                {
                    var executablePath = Environment.ProcessPath;
                    if (executablePath is null)
                        return Task.FromResult(false);

                    // The argument informs the app that it was launched on system startup so it can start in the background
                    runKey.SetValue(AUTOSTART_ENTRY_NAME, $"\"{executablePath}\" {UI.Constants.AUTOSTART_ARGUMENT}");
                }
                else
                    runKey.DeleteValue(AUTOSTART_ENTRY_NAME, throwOnMissingValue: false);

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
#elif WINDOWS
            return SetStartupTaskStateAsync(isEnabled);

            static async Task<bool> SetStartupTaskStateAsync(bool isEnabled)
            {
                var startupTask = await global::Windows.ApplicationModel.StartupTask.GetAsync(STARTUP_TASK_ID);
                if (!isEnabled)
                {
                    startupTask.Disable();
                    return true;
                }

                // The request is denied when the user explicitly disabled the startup task in system settings
                var state = await startupTask.RequestEnableAsync();
                return state
                    is global::Windows.ApplicationModel.StartupTaskState.Enabled
                    or global::Windows.ApplicationModel.StartupTaskState.EnabledByPolicy;
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
