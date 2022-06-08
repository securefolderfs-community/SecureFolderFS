using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.Sdk.SecureStore
{
    public abstract class LockedArray<T> : DisposableArray<T>
    {
        protected GCHandle handle;

        protected readonly uint length;

        protected bool isLocked;

        protected LockedArray(T[] bytes)
            : this(bytes, true)
        {
        }

        protected LockedArray(T[] bytes, bool preventDiskSwap) // TODO: Is `bytes` as parameter here safe? Pass `size` instead?
            : base(bytes)
        {
            length = (uint)(Marshal.SizeOf(default(T)) * base.bytes.Length);

            InitializeInternal(preventDiskSwap);
        }

        private void InitializeInternal(bool preventDiskSwap)
        {
            handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            if (preventDiskSwap)
            {
                isLocked = InitializeLock();
            }
        }

        protected void EnsureHandleFree()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }

        protected abstract bool InitializeLock();

        protected abstract bool IsSupported();
    }
}
