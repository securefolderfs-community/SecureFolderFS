using System;
using System.Runtime.InteropServices;
using Foundation;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class MacOsSystemService : ISystemService
    {
        private EventHandler? _desktopLocked;

        /// <inheritdoc/>
        public event EventHandler? DesktopLocked // TODO: Disabled on MacOS due to exception when adding a observer
        {
            add
            {
                return;

                _desktopLocked += value;
                NSNotificationCenter.DefaultCenter.AddObserver((NSString)"com.apple.screenIsLocked",
                    NSKeyValueObservingOptions.New, _ =>
                    {
                        _desktopLocked?.Invoke(this, EventArgs.Empty);
                    });
            }
            remove
            {
                return;

                _desktopLocked -= value;
                NSNotificationCenter.DefaultCenter.RemoveObserver((NSString)"com.apple.screenIsLocked");
            }
        }

        [DllImport("/System/Library/Frameworks/CoreServices.framework/CoreServices")]
        private static extern IntPtr CGSessionCopyCurrentDictionary();
    }
}
