#if __UNO_SKIA_MACOS__
using System;
using System.Runtime.InteropServices;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Uno.PInvoke;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed partial class SkiaSystemService
    {
        private IntPtr _notificationCenter;
        private IntPtr _notificationNameRef;
        private UnsafeNative.LockCallback? _lockCallback;
        
        /// <inheritdoc/>
        public event EventHandler? DeviceLocked
        {
            add
            {
                _deviceLocked += value;
                if (_deviceLocked?.GetInvocationList().Length == 1)
                    AttachLockObserver();
            }
            remove
            {
                _deviceLocked -= value;
                if (_deviceLocked is null)
                    DetachLockObserver();
            }
        }

        private void AttachLockObserver()
        {
            const uint kCFStringEncodingUTF8 = 0x08000100;

            _lockCallback = (_, _, _, _, _) => _deviceLocked?.Invoke(this, EventArgs.Empty);
            _notificationCenter = UnsafeNative.CFNotificationCenterGetDistributedCenter();
            _notificationNameRef = UnsafeNative.CFStringCreateWithCString(IntPtr.Zero, "com.apple.screenIsLocked", kCFStringEncodingUTF8);

            UnsafeNative.CFNotificationCenterAddObserver(
                _notificationCenter,
                observer: IntPtr.Zero,
                callback: Marshal.GetFunctionPointerForDelegate(_lockCallback),
                name: _notificationNameRef,
                obj: IntPtr.Zero,
                suspensionBehavior: UnsafeNative.CFNotificationSuspensionBehaviorDeliverImmediately);
        }

        private void DetachLockObserver()
        {
            UnsafeNative.CFNotificationCenterRemoveObserver(
                _notificationCenter,
                observer: IntPtr.Zero,
                name: _notificationNameRef,
                obj: IntPtr.Zero);

            _lockCallback = null;
        }
    }
}
#endif
