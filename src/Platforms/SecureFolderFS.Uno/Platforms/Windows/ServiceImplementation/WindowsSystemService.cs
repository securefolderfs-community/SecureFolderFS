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
                _desktopLocked += value;
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            }
            remove
            {
                _desktopLocked -= value;
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
