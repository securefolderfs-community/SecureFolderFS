using System;
using System.Diagnostics.Contracts;
using SecureFolderFS.Core.UnsafeNative;
using SecureFolderFS.Sdk.SecureStore;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.SecureStore
{
    internal sealed class Win32LockedArray<T> : LockedArray<T>, IExposedBuffer<T>
    {
        T[] IExposedBuffer<T>.Buffer => base.bytes;

        public Win32LockedArray(T[] bytes)
            : this(bytes, true)
        {
        }

        private Win32LockedArray(T[] bytes, bool preventDiskSwap)
            : base(bytes, preventDiskSwap)
        {
        }

        [Pure]
        protected override bool InitializeLock()
        {
            if (IsSupported() && this.handle.IsAllocated)
            {
                IntPtr pBuffer = this.handle.AddrOfPinnedObject();
                UIntPtr cnt = new(length);
                return UnsafeNativeApis.VirtualLock(pBuffer, cnt);
            }

            return false;
        }

        protected override void SecureFree()
        {
            try
            {
                if (IsSupported() && this.handle.IsAllocated)
                {
                    IntPtr pBuffer = this.handle.AddrOfPinnedObject();
                    UIntPtr cnt = new(length);
                    UnsafeNativeApis.RtlZeroMemory(pBuffer, cnt);

                    if (isLocked)
                    {
                        UnsafeNativeApis.VirtualUnlock(pBuffer, cnt);
                    }
                }
                else
                {
                    base.SecureFree();
                }
            }
            finally
            {
                EnsureHandleFree();
            }
        }

        protected override void NotSecureFree()
        {
            EnsureHandleFree();
        }

        protected override bool IsSupported()
        {
            return CompatibilityHelpers.IsPlatformWindows;
        }
    }
}
