using Android.Content;
using Android.OS;
using Android.OS.Storage;
using Android.Systems;
using Android.Util;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android
{
    public sealed class StorageManagerCompat
    {
        private const string TAG = nameof(StorageManagerCompat);

        private readonly StorageManager _storageManager;

        public StorageManagerCompat(Context context)
        {
            _storageManager = (StorageManager)context.GetSystemService(Context.StorageService)!;
        }

        public static StorageManagerCompat From(Context context) => new StorageManagerCompat(context);

        public ParcelFileDescriptor OpenProxyFileDescriptor(
            ParcelFileMode mode,
            ProxyFileDescriptorCallbackCompat callback,
            Handler handler)
        {
            // if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            // {
            //     return _storageManager.OpenProxyFileDescriptor(
            //         mode,
            //         callback.ToAndroidOsProxyFileDescriptorCallback(),
            //         handler
            //     );
            // }

            // Handle pre-Android O compatibility
            if (mode != ParcelFileMode.ReadOnly && mode != ParcelFileMode.WriteOnly)
            {
                throw new NotSupportedException($"Mode {mode} is not supported.");
            }

            var pipe = ParcelFileDescriptor.CreateReliablePipe();
            if (mode == ParcelFileMode.ReadOnly)
            {
                handler.Post(() =>
                {
                    try
                    {
                        using var outputStream = new ParcelFileDescriptor.AutoCloseOutputStream(pipe[1]);
                        var size = (int)callback.OnGetSize();
                        var buffer = new byte[size];
                        callback.OnRead(0, size, buffer);
                        outputStream.Write(buffer);
                        callback.OnRelease();
                    }
                    catch (Exception e)
                    {
                        Log.Error(TAG, "Failed to read file.", e);
                        pipe[1].CloseWithError(e.Message);
                    }
                });

                return pipe[0];
            }

            if (mode == ParcelFileMode.WriteOnly)
            {
                handler.Post(() =>
                {
                    try
                    {
                        using var inputStream = new ParcelFileDescriptor.AutoCloseInputStream(pipe[0]);
                        var buffer = new byte[inputStream.Available()];
                        inputStream.Read(buffer);
                        callback.OnWrite(0, buffer.Length, buffer);
                        callback.OnRelease();
                    }
                    catch (Exception e)
                    {
                        Log.Error(TAG, "Failed to write file.", e);
                        pipe[0].CloseWithError(e.Message);
                    }
                });

                return pipe[1];
            }

            // Should never reach here
            throw new NotSupportedException($"Mode {mode} is not supported.");
        }

        public abstract class ProxyFileDescriptorCallbackCompat
        {
            public virtual long OnGetSize() => throw new ErrnoException("onGetSize", OsConstants.Ebadf);

            public virtual int OnRead(long offset, int size, byte[] data) => throw new ErrnoException("onRead", OsConstants.Ebadf);

            public virtual int OnWrite(long offset, int size, byte[] data) => throw new ErrnoException("onWrite", OsConstants.Ebadf);

            public virtual void OnFsync() => throw new ErrnoException("onFsync", OsConstants.Einval);

            public abstract void OnRelease();

            public ProxyFileDescriptorCallback ToAndroidOsProxyFileDescriptorCallback()
            {
                return new ProxyFileDescriptorCallbackWrapper(this);
            }

            private class ProxyFileDescriptorCallbackWrapper : ProxyFileDescriptorCallback
            {
                private readonly ProxyFileDescriptorCallbackCompat _compat;

                public ProxyFileDescriptorCallbackWrapper(ProxyFileDescriptorCallbackCompat compat)
                {
                    _compat = compat;
                }

                public override int OnRead(long offset, int size, byte[] data) => _compat.OnRead(offset, size, data);

                public override int OnWrite(long offset, int size, byte[] data) => _compat.OnWrite(offset, size, data);

                public override long OnGetSize() => _compat.OnGetSize();

                public override void OnFsync() => _compat.OnFsync();

                public override void OnRelease() => _compat.OnRelease();
            }
        }
    }
}