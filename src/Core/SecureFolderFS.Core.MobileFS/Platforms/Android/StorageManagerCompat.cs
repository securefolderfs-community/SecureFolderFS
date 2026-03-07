using Android.Content;
using Android.OS;
using Android.OS.Storage;
using Android.Systems;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android
{
    public sealed class StorageManagerCompat
    {
        private readonly StorageManager _storageManager;

        public StorageManagerCompat(Context context)
        {
            _storageManager = (StorageManager)context.GetSystemService(Context.StorageService)!;
        }

        public ParcelFileDescriptor OpenProxyFileDescriptor(
            ParcelFileMode mode,
            ProxyFileDescriptorCallbackCompat callback,
            Handler handler)
        {
            return _storageManager.OpenProxyFileDescriptor(
                mode,
                callback.ToAndroidOsProxyFileDescriptorCallback(),
                handler
            );
        }

        public abstract class ProxyFileDescriptorCallbackCompat
        {
            public virtual long OnGetSize() => throw new ErrnoException("onGetSize", OsConstants.Ebadf);

            public virtual int OnRead(long offset, int size, byte[]? data) => throw new ErrnoException("onRead", OsConstants.Ebadf);

            public virtual int OnWrite(long offset, int size, byte[]? data) => throw new ErrnoException("onWrite", OsConstants.Ebadf);

            public virtual void OnFsync() => throw new ErrnoException("onFsync", OsConstants.Einval);

            public abstract void OnRelease();

            public ProxyFileDescriptorCallback ToAndroidOsProxyFileDescriptorCallback()
            {
                return new ProxyFileDescriptorCallbackWrapper(this);
            }

            private class ProxyFileDescriptorCallbackWrapper(ProxyFileDescriptorCallbackCompat compat)
                : ProxyFileDescriptorCallback
            {
                /// <inheritdoc/>
                public override int OnRead(long offset, int size, byte[]? data) => compat.OnRead(offset, size, data);

                /// <inheritdoc/>
                public override int OnWrite(long offset, int size, byte[]? data) => compat.OnWrite(offset, size, data);

                /// <inheritdoc/>
                public override long OnGetSize() => compat.OnGetSize();

                /// <inheritdoc/>
                public override void OnFsync() => compat.OnFsync();

                /// <inheritdoc/>
                public override void OnRelease() => compat.OnRelease();
            }
        }
    }
}