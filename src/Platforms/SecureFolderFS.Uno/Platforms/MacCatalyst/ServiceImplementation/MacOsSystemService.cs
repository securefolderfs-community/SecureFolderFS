using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class MacOsSystemService : BaseSystemService
    {
        [DllImport("/System/Library/Frameworks/CoreServices.framework/CoreServices")]
        private static extern IntPtr CGSessionCopyCurrentDictionary();

        /// <inheritdoc/>
        protected override void AttachEvent(ref EventHandler? handler, EventHandler? value)
        {
            handler += value;
            NSNotificationCenter.DefaultCenter.AddObserver((NSString)"com.apple.screenIsLocked",
                NSKeyValueObservingOptions.New, notification =>
                {
                    desktopLocked?.Invoke(this, EventArgs.Empty);
                });
        }

        /// <inheritdoc/>
        protected override void DetachEvent(ref EventHandler? handler, EventHandler? value)
        {
            handler -= value;
            NSNotificationCenter.DefaultCenter.RemoveObserver((NSString)"com.apple.screenIsLocked");
        }
    }
}
