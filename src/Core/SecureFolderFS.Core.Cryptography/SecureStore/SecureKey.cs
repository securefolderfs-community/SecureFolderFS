using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography.UnsafeNative;
using SecureFolderFS.Shared.ComponentModel;
using static SecureFolderFS.Shared.SharedConfiguration;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <summary>
    /// A secure key implementation that protects key material in memory.
    /// </summary>
    /// <remarks>
    /// When <see cref="UseMemoryHardening"/> is enabled, this class provides additional security measures:
    /// <list type="bullet">
    ///   <item>Memory is pinned to prevent GC from moving it (which would leave copies in memory)</item>
    ///   <item>Key material is XOR'd with a random mask, so the plaintext key never exists on the heap</item>
    ///   <item>When accessing the key via UseKey, it's de-XOR'd into a stack-allocated buffer</item>
    ///   <item>On Windows/Unix, memory pages are locked to prevent swapping to disk</item>
    ///   <item>On disposal, memory is securely zeroed using constant-time operations</item>
    /// </list>
    /// Without memory hardening, the key is stored in plaintext for maximum performance.
    /// </remarks>
    public sealed class SecureKey : IKeyUsage, ICloneable
    {
        // Maximum key size for stack allocation (256 bytes = 2048 bits, covers most crypto keys)
        private const int MAX_STACK_ALLOC_SIZE = 256;

        private readonly byte[] _obfuscatedKey;
        private readonly byte[]? _xorMask;
        private readonly bool _isMemoryLocked;
        private bool _disposed;

        /// <inheritdoc/>
        public int Length { get; }

        /// <summary>
        /// Gets a value indicating whether this key has been disposed of.
        /// </summary>
        public bool IsDisposed => _disposed;

        private SecureKey(byte[] key, bool takeOwnership)
        {
            Length = key.Length;

            if (UseMemoryHardening)
            {
                // Always create a new secure copy with XOR obfuscation
                _obfuscatedKey = GC.AllocateArray<byte>(key.Length, pinned: true);
                _xorMask = GC.AllocateArray<byte>(key.Length, pinned: true);
                RandomNumberGenerator.Fill(_xorMask);

                _isMemoryLocked = TryLockMemory(_obfuscatedKey) && TryLockMemory(_xorMask);

                // XOR the key with mask and store
                XorBytes(key.AsSpan(), _xorMask.AsSpan(), _obfuscatedKey.AsSpan());

                // If we took ownership, zero the original
                if (takeOwnership)
                    CryptographicOperations.ZeroMemory(key);
            }
            else
            {
                if (takeOwnership)
                {
                    _obfuscatedKey = key;
                }
                else
                {
                    _obfuscatedKey = new byte[key.Length];
                    key.AsSpan().CopyTo(_obfuscatedKey);
                }
            }
        }

        /// <inheritdoc/>
        public void UseKey(Action<ReadOnlySpan<byte>> keyAction)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(keyAction);

            if (!UseMemoryHardening || _xorMask is null)
            {
                // Fast path: no obfuscation, just use the key directly
                keyAction(_obfuscatedKey.AsSpan());
                return;
            }

            // Slow path with XOR obfuscation: de-XOR into stack buffer
            if (Length <= MAX_STACK_ALLOC_SIZE)
            {
                // Stack allocate for small keys
                Span<byte> tempKey = stackalloc byte[Length];
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey);
                    keyAction(tempKey);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
            else
            {
                // For larger keys, use a pinned array (rare case)
                var tempKey = GC.AllocateArray<byte>(Length, pinned: true);
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey.AsSpan());
                    keyAction(tempKey.AsSpan());
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
        }

        /// <inheritdoc/>
        public void UseKey<TState>(TState state, ReadOnlySpanAction<byte, TState> keyAction)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(keyAction);

            if (!UseMemoryHardening || _xorMask is null)
            {
                // Fast path: no obfuscation, just use the key directly
                keyAction(_obfuscatedKey.AsSpan(), state);
                return;
            }

            // Slow path with XOR obfuscation: de-XOR into stack buffer
            if (Length <= MAX_STACK_ALLOC_SIZE)
            {
                // Stack allocate for small keys
                Span<byte> tempKey = stackalloc byte[Length];
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey);
                    keyAction(tempKey, state);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
            else
            {
                // For larger keys, use a pinned array (rare case)
                var tempKey = GC.AllocateArray<byte>(Length, pinned: true);
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey.AsSpan());
                    keyAction(tempKey.AsSpan(), state);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
        }

        /// <inheritdoc/>
        public TResult UseKey<TResult>(Func<ReadOnlySpan<byte>, TResult> keyAction)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(keyAction);

            if (!UseMemoryHardening || _xorMask is null)
            {
                // Fast path: no obfuscation, just use the key directly
                return keyAction(_obfuscatedKey.AsSpan());
            }

            // Slow path with XOR obfuscation: de-XOR into stack buffer
            if (Length <= MAX_STACK_ALLOC_SIZE)
            {
                // Stack allocate for small keys
                Span<byte> tempKey = stackalloc byte[Length];
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey);
                    return keyAction(tempKey);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
            else
            {
                // For larger keys, use a pinned array (rare case)
                var tempKey = GC.AllocateArray<byte>(Length, pinned: true);
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey.AsSpan());
                    return keyAction(tempKey.AsSpan());
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
        }

        /// <inheritdoc/>
        public TResult UseKey<TState, TResult>(TState state, ReadOnlySpanFunc<byte, TState, TResult> keyAction)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(keyAction);

            if (!UseMemoryHardening || _xorMask is null)
            {
                // Fast path: no obfuscation, just use the key directly
                return keyAction(_obfuscatedKey.AsSpan(), state);
            }

            // Slow path with XOR obfuscation: de-XOR into stack buffer
            if (Length <= MAX_STACK_ALLOC_SIZE)
            {
                // Stack allocate for small keys
                Span<byte> tempKey = stackalloc byte[Length];
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey);
                    return keyAction(tempKey, state);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
            else
            {
                // For larger keys, use a pinned array (rare case)
                var tempKey = GC.AllocateArray<byte>(Length, pinned: true);
                try
                {
                    XorBytes(_obfuscatedKey.AsSpan(), _xorMask.AsSpan(), tempKey.AsSpan());
                    return keyAction(tempKey, state);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempKey);
                }
            }
        }

        /// <inheritdoc/>
        public object Clone()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // De-XOR and create a new copy
            return UseKey(key =>
            {
                var copy = new byte[Length];
                key.CopyTo(copy);
                return new SecureKey(copy, takeOwnership: true);
            });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Securely zero the memory using constant-time operation
            CryptographicOperations.ZeroMemory(_obfuscatedKey);
            if (_xorMask is not null)
                CryptographicOperations.ZeroMemory(_xorMask);

            if (UseMemoryHardening)
            {
                // Unlock memory pages if they were locked
                if (_isMemoryLocked)
                {
                    TryUnlockMemory(_obfuscatedKey);
                    if (_xorMask is not null)
                        TryUnlockMemory(_xorMask);
                }

                // Note: Arrays allocated with GC.AllocateArray(pinned: true) are on the POH
                // They don't need explicit unpinning - they're freed when GC collects them
            }
        }

        /// <summary>
        /// Takes the ownership of the provided key and manages its lifetime.
        /// </summary>
        /// <param name="key">The key to import.</param>
        /// <remarks>
        /// When memory hardening is enabled, the key will be XOR-obfuscated immediately,
        /// and the original array will be securely zeroed.
        /// </remarks>
        public static SecureKey TakeOwnership(byte[] key)
        {
            return new SecureKey(key, takeOwnership: true);
        }

        /// <summary>
        /// Creates a new <see cref="SecureKey"/> by copying data from the provided key.
        /// </summary>
        /// <param name="key">The key to copy from.</param>
        public static SecureKey FromCopy(byte[] key)
        {
            return new SecureKey(key, takeOwnership: false);
        }

        /// <summary>
        /// Creates a new <see cref="SecureKey"/> filled with cryptographically secure random bytes.
        /// </summary>
        /// <param name="size">The size of the key in bytes.</param>
        /// <returns>A new <see cref="SecureKey"/> containing random key material.</returns>
        public static SecureKey CreateSecureRandom(int size)
        {
            var randomBytes = new byte[size];
            RandomNumberGenerator.Fill(randomBytes);

            return new SecureKey(randomBytes, takeOwnership: true);
        }

        /// <summary>
        /// Creates a new <see cref="SecureKey"/> and copies the data from a span of bytes.
        /// </summary>
        /// <param name="key">The key data to copy.</param>
        /// <returns>A new <see cref="SecureKey"/> containing the provided key material.</returns>
        public static SecureKey FromSpanCopy(ReadOnlySpan<byte> key)
        {
            var copy = new byte[key.Length];
            key.CopyTo(copy);
            return new SecureKey(copy, takeOwnership: true);
        }

        /// <summary>
        /// XORs source with mask and writes to destination.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void XorBytes(ReadOnlySpan<byte> source, ReadOnlySpan<byte> mask, Span<byte> destination)
        {
            for (var i = 0; i < source.Length; i++)
                destination[i] = (byte)(source[i] ^ mask[i]);
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
        private static unsafe bool TryLockMemoryWindows(byte[] buffer)
        {
            // VirtualLock prevents pages from being swapped to the pagefile
            // The buffer is already pinned (allocated on POH), so we can safely get its address
            fixed (byte* ptr = buffer)
            {
                return UnsafeNativeApis.VirtualLock((nint)ptr, (nuint)buffer.Length);
            }
        }

        [SupportedOSPlatform("windows")]
        private static unsafe bool TryUnlockMemoryWindows(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
            {
                return UnsafeNativeApis.VirtualUnlock((nint)ptr, (nuint)buffer.Length);
            }
        }

        #endregion

        #region Unix Memory Locking (Linux/macOS)

        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        private static unsafe bool TryLockMemoryUnix(byte[] buffer)
        {
            // mlock prevents pages from being swapped out
            // The buffer is already pinned (allocated on POH), so we can safely get its address
            fixed (byte* ptr = buffer)
            {
                return UnsafeNativeApis.mlock((nint)ptr, (nuint)buffer.Length) == 0;
            }
        }

        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        private static unsafe bool TryUnlockMemoryUnix(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
            {
                return UnsafeNativeApis.munlock((nint)ptr, (nuint)buffer.Length) == 0;
            }
        }

        #endregion

        #endregion
    }
}
