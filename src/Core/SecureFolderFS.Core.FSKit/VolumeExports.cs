#pragma warning disable APL0002
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FSKit;

namespace SecureFolderFS.Core.FSKit
{
    /// <summary>
    /// Provides native entry points for FSVolume operations that can be called from Swift.
    /// </summary>
    public static class VolumeExports
    {
        private static readonly Dictionary<IntPtr, Callbacks.MacOsVolume> _volumes = new();
        private static long _nextHandleId = 1;
        private static readonly object _lock = new();

        // POSIX error codes
        private const int EINVAL = 22;   // Invalid argument
        private const int ENOENT = 2;    // No such file or directory
        private const int ENOSYS = 38;   // Function not implemented
        private const int EROFS = 30;    // Read-only file system
        private const int EIO = 5;       // I/O error
        private const int EEXIST = 17;   // File exists
        private const int ENOTDIR = 20;  // Not a directory
        private const int EISDIR = 21;   // Is a directory
        private const int ENOTEMPTY = 66; // Directory not empty

        /// <summary>
        /// Creates a new volume instance and returns a handle to it.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_Create")]
        public static IntPtr CreateVolume(IntPtr volumeNamePtr, int volumeNameLength)
        {
            try
            {
                if (volumeNamePtr == IntPtr.Zero || volumeNameLength <= 0)
                {
                    Console.WriteLine($"FSVolume_Create: Invalid parameters");
                    return IntPtr.Zero;
                }

                // Get volume name from pointer
                var volumeNameBytes = new byte[volumeNameLength];
                Marshal.Copy(volumeNamePtr, volumeNameBytes, 0, volumeNameLength);
                var volumeName = Encoding.UTF8.GetString(volumeNameBytes);

                Console.WriteLine($"FSVolume_Create: Creating volume '{volumeName}'");

                // Create FSKit objects
                var volumeId = new FSVolumeIdentifier();
                var fsVolumeName = FSFileName.Create(volumeName);

                // Create our C# volume implementation
                var volume = new Callbacks.MacOsVolume(volumeId, fsVolumeName);

                // Store in dictionary and return handle
                lock (_lock)
                {
                    var handle = new IntPtr(_nextHandleId++);
                    _volumes[handle] = volume;
                    Console.WriteLine($"FSVolume_Create: Created volume handle {handle} for '{volumeName}'");
                    return handle;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_Create: Exception: {ex}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Destroys a volume instance.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_Destroy")]
        public static void DestroyVolume(IntPtr handle)
        {
            try
            {
                lock (_lock)
                {
                    if (_volumes.TryGetValue(handle, out var volume))
                    {
                        // Perform any cleanup needed
                        // volume.Dispose() if you implement IDisposable

                        _volumes.Remove(handle);
                        Console.WriteLine($"FSVolume_Destroy: Destroyed volume handle {handle}");
                    }
                    else
                    {
                        Console.WriteLine($"FSVolume_Destroy: Volume handle {handle} not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_Destroy: Exception: {ex}");
            }
        }

        /// <summary>
        /// Activates a volume.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_Activate")]
        public static int Activate(IntPtr handle)
        {
            try
            {
                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_Activate: Volume handle {handle} not found");
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_Activate: Activating volume {handle}");

                // TODO: Implement actual activation logic
                // Example: volume.Activate();

                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_Activate: Exception: {ex}");
                return -EIO;
            }
        }

        /// <summary>
        /// Deactivates a volume.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_Deactivate")]
        public static int Deactivate(IntPtr handle)
        {
            try
            {
                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_Deactivate: Volume handle {handle} not found");
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_Deactivate: Deactivating volume {handle}");

                // TODO: Implement actual deactivation logic
                // Example: volume.Deactivate();

                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_Deactivate: Exception: {ex}");
                return -EIO;
            }
        }

        /// <summary>
        /// Unmounts a volume.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_Unmount")]
        public static int Unmount(IntPtr handle)
        {
            try
            {
                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_Unmount: Volume handle {handle} not found");
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_Unmount: Unmounting volume {handle}");

                // TODO: Implement actual unmount logic
                // Example: volume.Unmount();

                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_Unmount: Exception: {ex}");
                return -EIO;
            }
        }

        /// <summary>
        /// Looks up an item in a directory.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_LookupItem")]
        public static unsafe int LookupItem(IntPtr handle, IntPtr namePtr, int nameLength, long directoryId, long* itemIdPtr)
        {
            try
            {
                if (namePtr == IntPtr.Zero || nameLength <= 0 || itemIdPtr == null)
                {
                    Console.WriteLine($"FSVolume_LookupItem: Invalid parameters");
                    if (itemIdPtr != null) *itemIdPtr = 0;
                    return -EINVAL;
                }

                var nameBytes = new byte[nameLength];
                Marshal.Copy(namePtr, nameBytes, 0, nameLength);
                var name = Encoding.UTF8.GetString(nameBytes);

                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_LookupItem: Volume handle {handle} not found");
                        *itemIdPtr = 0;
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_LookupItem: Looking up '{name}' in directory {directoryId}");

                // TODO: Implement actual lookup logic
                // Example:
                // var result = volume.LookupItem(name, directoryId);
                // if (result.Success)
                // {
                //     *itemIdPtr = result.ItemId;
                //     return 0;
                // }
                // return -ENOENT;

                *itemIdPtr = 0;
                return -ENOENT; // Not found (for now)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_LookupItem: Exception: {ex}");
                if (itemIdPtr != null) *itemIdPtr = 0;
                return -EIO;
            }
        }

        /// <summary>
        /// Gets attributes of an item.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_GetAttributes")]
        public static int GetAttributes(IntPtr handle, long itemId, IntPtr attributesPtr)
        {
            try
            {
                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_GetAttributes: Volume handle {handle} not found");
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_GetAttributes: Getting attributes for item {itemId}");

                // TODO: Implement actual attribute fetching
                // If attributesPtr is not null, you could marshal attribute data back
                // For now, just return success to indicate the item exists

                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_GetAttributes: Exception: {ex}");
                return -EIO;
            }
        }

        /// <summary>
        /// Reads data from a file.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_Read")]
        public static unsafe int Read(IntPtr handle, long itemId, long offset, IntPtr buffer, int length, int* bytesReadPtr)
        {
            try
            {
                if (buffer == IntPtr.Zero || length <= 0 || bytesReadPtr == null)
                {
                    Console.WriteLine($"FSVolume_Read: Invalid parameters");
                    if (bytesReadPtr != null) *bytesReadPtr = 0;
                    return -EINVAL;
                }

                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_Read: Volume handle {handle} not found");
                        *bytesReadPtr = 0;
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_Read: Reading {length} bytes from item {itemId} at offset {offset}");

                // TODO: Implement actual read logic
                // Example:
                // var data = volume.Read(itemId, offset, length);
                // if (data != null)
                // {
                //     Marshal.Copy(data, 0, buffer, data.Length);
                //     *bytesReadPtr = data.Length;
                //     return 0;
                // }

                *bytesReadPtr = 0;
                return -ENOSYS; // Not implemented
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_Read: Exception: {ex}");
                if (bytesReadPtr != null) *bytesReadPtr = 0;
                return -EIO;
            }
        }

        /// <summary>
        /// Writes data to a file.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_Write")]
        public static unsafe int Write(IntPtr handle, long itemId, long offset, IntPtr buffer, int length, int* bytesWrittenPtr)
        {
            try
            {
                if (buffer == IntPtr.Zero || length <= 0 || bytesWrittenPtr == null)
                {
                    Console.WriteLine($"FSVolume_Write: Invalid parameters");
                    if (bytesWrittenPtr != null) *bytesWrittenPtr = 0;
                    return -EINVAL;
                }

                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_Write: Volume handle {handle} not found");
                        *bytesWrittenPtr = 0;
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_Write: Writing {length} bytes to item {itemId} at offset {offset}");

                // TODO: Implement actual write logic
                // Example:
                // var data = new byte[length];
                // Marshal.Copy(buffer, data, 0, length);
                // var written = volume.Write(itemId, offset, data);
                // *bytesWrittenPtr = written;
                // return 0;

                *bytesWrittenPtr = 0;
                return -EROFS; // Read-only for now
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_Write: Exception: {ex}");
                if (bytesWrittenPtr != null) *bytesWrittenPtr = 0;
                return -EIO;
            }
        }

        /// <summary>
        /// Creates a new item (file or directory).
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_CreateItem")]
        public static unsafe int CreateItem(IntPtr handle, IntPtr namePtr, int nameLength, int itemType, long directoryId, long* newItemIdPtr)
        {
            try
            {
                if (namePtr == IntPtr.Zero || nameLength <= 0 || newItemIdPtr == null)
                {
                    Console.WriteLine($"FSVolume_CreateItem: Invalid parameters");
                    if (newItemIdPtr != null) *newItemIdPtr = 0;
                    return -EINVAL;
                }

                var nameBytes = new byte[nameLength];
                Marshal.Copy(namePtr, nameBytes, 0, nameLength);
                var name = Encoding.UTF8.GetString(nameBytes);

                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_CreateItem: Volume handle {handle} not found");
                        *newItemIdPtr = 0;
                        return -EINVAL;
                    }
                }

                var typeStr = itemType == 1 ? "directory" : "file";
                Console.WriteLine($"FSVolume_CreateItem: Creating {typeStr} '{name}' in directory {directoryId}");

                // TODO: Implement actual create logic
                // Example:
                // var isDirectory = itemType == 1;
                // var newItemId = volume.CreateItem(name, directoryId, isDirectory);
                // if (newItemId > 0)
                // {
                //     *newItemIdPtr = newItemId;
                //     return 0;
                // }

                *newItemIdPtr = 0;
                return -EROFS; // Read-only for now
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_CreateItem: Exception: {ex}");
                if (newItemIdPtr != null) *newItemIdPtr = 0;
                return -EIO;
            }
        }

        /// <summary>
        /// Removes an item.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_RemoveItem")]
        public static int RemoveItem(IntPtr handle, long itemId, IntPtr namePtr, int nameLength, long directoryId)
        {
            try
            {
                if (namePtr == IntPtr.Zero || nameLength <= 0)
                {
                    Console.WriteLine($"FSVolume_RemoveItem: Invalid parameters");
                    return -EINVAL;
                }

                var nameBytes = new byte[nameLength];
                Marshal.Copy(namePtr, nameBytes, 0, nameLength);
                var name = Encoding.UTF8.GetString(nameBytes);

                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_RemoveItem: Volume handle {handle} not found");
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_RemoveItem: Removing '{name}' (item {itemId}) from directory {directoryId}");

                // TODO: Implement actual remove logic
                // Example:
                // var success = volume.RemoveItem(itemId, directoryId);
                // return success ? 0 : -ENOENT;

                return -EROFS; // Read-only for now
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_RemoveItem: Exception: {ex}");
                return -EIO;
            }
        }

        /// <summary>
        /// Enumerates directory contents.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "FSVolume_EnumerateDirectory")]
        public static int EnumerateDirectory(IntPtr handle, long directoryId, long startingCookie, IntPtr callbackPtr)
        {
            try
            {
                Callbacks.MacOsVolume? volume;
                lock (_lock)
                {
                    if (!_volumes.TryGetValue(handle, out volume))
                    {
                        Console.WriteLine($"FSVolume_EnumerateDirectory: Volume handle {handle} not found");
                        return -EINVAL;
                    }
                }

                Console.WriteLine($"FSVolume_EnumerateDirectory: Enumerating directory {directoryId} starting at cookie {startingCookie}");

                // TODO: Implement actual enumeration logic
                // Example:
                // var entries = volume.EnumerateDirectory(directoryId, startingCookie);
                // if (callbackPtr != IntPtr.Zero)
                // {
                //     // Call back to Swift with each entry
                //     // This would require defining a callback delegate
                // }
                // return entries.Count > 0 ? entries.Count : 0;

                return 0; // Success with no entries (empty directory)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSVolume_EnumerateDirectory: Exception: {ex}");
                return -EIO;
            }
        }

        /// <summary>
        /// Helper method to get volume from handle (internal use).
        /// </summary>
        private static bool TryGetVolume(IntPtr handle, out Callbacks.MacOsVolume? volume)
        {
            lock (_lock)
            {
                return _volumes.TryGetValue(handle, out volume);
            }
        }
    }
}