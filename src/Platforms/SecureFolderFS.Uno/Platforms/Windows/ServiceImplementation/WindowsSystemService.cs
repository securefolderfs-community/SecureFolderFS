using System;
using Microsoft.Win32;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class WindowsSystemService : ISystemService
    {
        private EventHandler? _desktopLocked;

        /// <inheritdoc/>
        public event EventHandler? DesktopLocked
        {
            add
            {
                if (_desktopLocked is null)
                    SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

                _desktopLocked += value;
            }
            remove
            {
                _desktopLocked -= value;
                if (_desktopLocked is null)
                    SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            }
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                    _desktopLocked?.Invoke(sender, e);
                    break;

                default: return;
            }
        }
    }
}
