using System;
using System.IO;

#if __UNO_SKIA_MACOS__
using SecureFolderFS.Uno.PInvoke;
#endif

namespace SecureFolderFS.Uno.Platforms.Desktop.Helpers
{
    internal static class MacOsIconHelper
    {
#if __UNO_SKIA_MACOS__
        public static void SetDockIcon(string iconPath)
        {
            if (!File.Exists(iconPath))
                return;

            try
            {
                // Get NSApplication sharedApplication
                var nsApplicationClass = UnsafeNative.objc_getClass("NSApplication");
                var sharedApplicationSelector = UnsafeNative.sel_registerName("sharedApplication");
                var sharedApplication = UnsafeNative.objc_msgSend_IntPtr(nsApplicationClass, sharedApplicationSelector);

                // Get NSImage and initialize with contentsOfFile
                var nsImageClass = UnsafeNative.objc_getClass("NSImage");
                var allocSelector = UnsafeNative.sel_registerName("alloc");
                var initWithContentsOfFileSelector = UnsafeNative.sel_registerName("initWithContentsOfFile:");
                
                var nsImage = UnsafeNative.objc_msgSend_IntPtr(nsImageClass, allocSelector);
                
                // Create NSString for the path
                var nsStringClass = UnsafeNative.objc_getClass("NSString");
                var stringWithUTF8StringSelector = UnsafeNative.sel_registerName("stringWithUTF8String:");
                var iconPathPtr = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(iconPath);
                var nsString = UnsafeNative.objc_msgSend_IntPtr_IntPtr(nsStringClass, stringWithUTF8StringSelector, iconPathPtr);
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(iconPathPtr);
                
                var imageWithPath = UnsafeNative.objc_msgSend_IntPtr_IntPtr(nsImage, initWithContentsOfFileSelector, nsString);

                // Set the application icon
                var setApplicationIconImageSelector = UnsafeNative.sel_registerName("setApplicationIconImage:");
                UnsafeNative.objc_msgSend_IntPtr_IntPtr(sharedApplication, setApplicationIconImageSelector, imageWithPath);
            }
            catch (Exception)
            {
            }
        }
#endif
    }
}
