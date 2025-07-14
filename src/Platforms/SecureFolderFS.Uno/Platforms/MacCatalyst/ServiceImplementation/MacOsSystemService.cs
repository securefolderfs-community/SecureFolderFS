using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class MacOsSystemService : ISystemService
    {
        private EventHandler? _deviceLocked;

        /// <inheritdoc/>
        public event EventHandler? DeviceLocked // TODO: Disabled on MacOS due to exception when adding a observer
        {
            add
            {
                return;

                _deviceLocked += value;
                NSNotificationCenter.DefaultCenter.AddObserver((NSString)"com.apple.screenIsLocked",
                    NSKeyValueObservingOptions.New, _ =>
                    {
                        _deviceLocked?.Invoke(this, EventArgs.Empty);
                    });
            }
            remove
            {
                return;

                _deviceLocked -= value;
                NSNotificationCenter.DefaultCenter.RemoveObserver((NSString)"com.apple.screenIsLocked");
            }
        }

        /// <inheritdoc/>
        public Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
            // TODO: Implement size calculation
            return Task.FromResult<long>(0);
        }

        [DllImport("/System/Library/Frameworks/CoreServices.framework/CoreServices")]
        private static extern IntPtr CGSessionCopyCurrentDictionary();
    }
}
