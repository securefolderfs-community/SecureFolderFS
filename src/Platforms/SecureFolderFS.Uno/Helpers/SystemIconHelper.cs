using System;
using System.IO;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

#if WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;
#elif __UNO_SKIA_MACOS__
using System.Runtime.InteropServices;
using SecureFolderFS.Uno.PInvoke;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;
#endif

namespace SecureFolderFS.Uno.Helpers
{
    /// <summary>
    /// Extracts the icon that the shell associates with a kind of item.
    /// </summary>
    internal static partial class SystemIconHelper
    {
        /// <summary>
        /// Gets the shell icon for an item of the given kind, encoded as a PNG.
        /// </summary>
        /// <param name="itemName">The name of the item whose kind determines the icon.</param>
        /// <param name="isFolder">Determines whether the icon should represent a folder.</param>
        /// <param name="size">The requested icon size, in pixels.</param>
        /// <returns>If the platform can provide one, returns the icon; otherwise null.</returns>
        public static IImage? TryGetIcon(string itemName, bool isFolder, uint size)
        {
            try
            {
                var pngData = GetIconData(itemName, isFolder, size);
                return pngData is null ? null : new StreamImageModel(new MemoryStream(pngData));
            }
            catch (Exception ex)
            {
                _ = ex;
                return null;
            }
        }

#if WINDOWS
        private static byte[]? GetIconData(string itemName, bool isFolder, uint size)
        {
            _ = size;
            var fileInfo = new SHFILEINFO();
            var attributes = isFolder ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;

            // SHGFI_USEFILEATTRIBUTES makes the shell answer from the name and attributes alone,
            // so the path does not have to exist and no I/O is performed
            var result = SHGetFileInfo(
                itemName,
                attributes,
                ref fileInfo,
                (uint)Marshal.SizeOf<SHFILEINFO>(),
                SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES);

            if (result == IntPtr.Zero || fileInfo.hIcon == IntPtr.Zero)
                return null;

            try
            {
                using var icon = Icon.FromHandle(fileInfo.hIcon);
                using var bitmap = icon.ToBitmap();
                using var memoryStream = new MemoryStream();

                bitmap.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
            finally
            {
                DestroyIcon(fileInfo.hIcon);
            }
        }
#elif __UNO_SKIA_MACOS__
        private static byte[]? GetIconData(string itemName, bool isFolder, uint size)
        {
            var workspace = objc_msgSend_IntPtr(objc_getClass("NSWorkspace"), sel_registerName("sharedWorkspace"));
            if (workspace == IntPtr.Zero)
                return null;

            // Folders share a single icon; files are keyed by extension, which is how Finder resolves them too
            var typeName = isFolder ? "public.folder" : Path.GetExtension(itemName).TrimStart('.');
            if (string.IsNullOrEmpty(typeName))
                typeName = "public.data";

            var typeString = CfString(typeName);
            var rectHandle = Marshal.AllocHGlobal(Marshal.SizeOf<CGRect>());
            var imageRep = IntPtr.Zero;
            try
            {
                var icon = objc_msgSend_IntPtr_IntPtr(workspace, sel_registerName("iconForFileType:"), typeString);
                if (icon == IntPtr.Zero)
                    return null;

                // The multi-resolution NSImage is rasterized for the requested rect, instead of taking the
                // first representation out of a TIFF dump, whose ordering is not guaranteed to be the largest
                Marshal.StructureToPtr(new CGRect { Width = size, Height = size }, rectHandle, false);
                var cgImage = objc_msgSend_IntPtr_IntPtr_IntPtr_IntPtr(
                    icon,
                    sel_registerName("CGImageForProposedRect:context:hints:"),
                    rectHandle,
                    IntPtr.Zero,
                    IntPtr.Zero);

                if (cgImage == IntPtr.Zero)
                    return null;

                var allocated = objc_msgSend_IntPtr(objc_getClass("NSBitmapImageRep"), sel_registerName("alloc"));
                if (allocated == IntPtr.Zero)
                    return null;

                imageRep = objc_msgSend_IntPtr_IntPtr(allocated, sel_registerName("initWithCGImage:"), cgImage);
                if (imageRep == IntPtr.Zero)
                    return null;

                var properties = objc_msgSend_IntPtr(objc_getClass("NSDictionary"), sel_registerName("dictionary"));
                var pngData = objc_msgSend_IntPtr_ulong_IntPtr(imageRep, sel_registerName("representationUsingType:properties:"), NSBitmapImageFileTypePng, properties);
                if (pngData == IntPtr.Zero)
                    return null;

                // NSData is toll-free bridged to CFData, so the Core Foundation accessors apply
                var length = (int)CFDataGetLength(pngData);
                var bytePointer = CFDataGetBytePtr(pngData);
                if (length <= 0 || bytePointer == IntPtr.Zero)
                    return null;

                var buffer = new byte[length];
                Marshal.Copy(bytePointer, buffer, 0, length);

                return buffer;
            }
            finally
            {
                if (imageRep != IntPtr.Zero)
                    objc_msgSend_void(imageRep, sel_registerName("release"));

                Marshal.FreeHGlobal(rectHandle);
                CFRelease(typeString);
            }
        }
#else
        private static byte[]? GetIconData(string itemName, bool isFolder, uint size)
        {
            _ = itemName;
            _ = isFolder;
            _ = size;

            return null;
        }
#endif
    }
}
