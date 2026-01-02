using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography.UnsafeNative;
using static SecureFolderFS.Shared.SharedConfiguration;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <inheritdoc cref="SecretKey"/>
    /// <remarks>
    /// When <see cref="SecureFolderFS.Shared.SharedConfiguration.UseMemoryHardening"/> is enabled, this class provides additional security measures:
    /// <list type="bullet">
    ///   <item>Memory is pinned to prevent GC from moving it (which would leave copies in memory)</item>
    ///   <item>On Windows, memory pages are locked to prevent swapping to disk (VirtualLock)</item>
    ///   <item>On disposal, memory is securely zeroed using constant-time operations</item>
    /// </list>
    /// </remarks>
    public sealed class SecureKey : SecretKey
    {
        private GCHandle _pinnedHandle;
        private readonly bool _isMemoryLocked;
        private bool _disposed;

        /// <inheritdoc/>
        public override byte[] Key { get; }

        /// <summary>
        /// Gets a value indicating whether this key has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        public SecureKey(int size)
        {
            // Allocate pinned array to prevent GC from moving it in memory
            // This prevents leaving copies of sensitive data scattered in heap
            Key = UseMemoryHardening
                ? GC.AllocateArray<byte>(size, pinned: true)
                : new byte[size];

            if (UseMemoryHardening)
            {
                _pinnedHandle = GCHandle.Alloc(Key, GCHandleType.Pinned);
                _isMemoryLocked = TryLockMemory(Key);
            }
        }

        private SecureKey(byte[] key, bool takeOwnership)
        {
            if (takeOwnership)
            {
                // Taking ownership of existing array - pin it but can't guarantee it wasn't already copied
                Key = key;
                if (UseMemoryHardening)
                {
                    _pinnedHandle = GCHandle.Alloc(Key, GCHandleType.Pinned);
                    _isMemoryLocked = TryLockMemory(Key);
                }
            }
            else
            {
                // Create a new secure copy
                Key = UseMemoryHardening
                    ? GC.AllocateArray<byte>(key.Length, pinned: true)
                    : new byte[key.Length];

                if (UseMemoryHardening)
                {
                    _pinnedHandle = GCHandle.Alloc(Key, GCHandleType.Pinned);
                    _isMemoryLocked = TryLockMemory(Key);
                }

                // Copy data securely
                key.AsSpan().CopyTo(Key);
            }
        }

        /// <inheritdoc/>
        public override SecretKey CreateCopy()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return new SecureKey(Key, takeOwnership: false);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Securely zero the memory using constant-time operation
            CryptographicOperations.ZeroMemory(Key);

            if (UseMemoryHardening)
            {
                // Unlock memory pages if they were locked
                if (_isMemoryLocked)
                    TryUnlockMemory(Key);

                // Release the pinned handle
                if (_pinnedHandle.IsAllocated)
                    _pinnedHandle.Free();
            }
        }

        /// <summary>
        /// Takes the ownership of the provided key and manages its lifetime.
        /// </summary>
        /// <param name="key">The key to import.</param>
        /// <remarks>
        /// Warning: When memory hardening is enabled, the original array will be pinned,
        /// but copies may already exist in memory from before this call.
        /// For maximum security, prefer creating keys directly with <see cref="SecureKey(int)"/>
        /// and filling them in place.
        /// </remarks>
        public static SecretKey TakeOwnership(byte[] key)
        {
            return new SecureKey(key, takeOwnership: true);
        }

        #region Platform-specific memory locking

        /// <summary>
        /// Attempts to lock memory pages to prevent them from being swapped to disk.
        /// This protects against cold boot attacks and forensic recovery from swap files.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryLockMemory(byte[] buffer)
        {
            if (!UseMemoryHardening || buffer.Length == 0)
                return false;

            try
            {
                if (OperatingSystem.IsWindows())
                    return TryLockMemoryWindows(buffer);

                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    return TryLockMemoryUnix(buffer);
            }
            catch
            {
                // Silently fail - memory locking is a best-effort security enhancement
                // The application should still work without it (e.g., insufficient privileges)
            }

            return false;
        }

        /// <summary>
        /// Attempts to unlock previously locked memory pages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryUnlockMemory(byte[] buffer)
        {
            if (!UseMemoryHardening || buffer.Length == 0)
                return;

            try
            {
                if (OperatingSystem.IsWindows())
                    TryUnlockMemoryWindows(buffer);
                else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    TryUnlockMemoryUnix(buffer);
            }
            catch
            {
                // Silently fail - unlocking failure is not critical
            }
        }

        #region Windows Memory Locking

        [SupportedOSPlatform("windows")]
        private static bool TryLockMemoryWindows(byte[] buffer)
        {
            // VirtualLock prevents pages from being swapped to the pagefile
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return UnsafeNativeApis.VirtualLock(handle.AddrOfPinnedObject(), (nuint)buffer.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        [SupportedOSPlatform("windows")]
        private static void TryUnlockMemoryWindows(byte[] buffer)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                UnsafeNativeApis.VirtualUnlock(handle.AddrOfPinnedObject(), (nuint)buffer.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        #endregion

        #region Unix Memory Locking (Linux/macOS)

        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        private static bool TryLockMemoryUnix(byte[] buffer)
        {
            // mlock prevents pages from being swapped out
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return UnsafeNativeApis.mlock(handle.AddrOfPinnedObject(), (nuint)buffer.Length) == 0;
            }
            finally
            {
                handle.Free();
            }
        }

        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        private static void TryUnlockMemoryUnix(byte[] buffer)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                UnsafeNativeApis.munlock(handle.AddrOfPinnedObject(), (nuint)buffer.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        #endregion


        #endregion
    }
}
