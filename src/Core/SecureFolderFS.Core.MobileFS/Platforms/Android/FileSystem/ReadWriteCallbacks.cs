using Android.OS;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    internal sealed class ReadWriteCallbacks : StorageManagerCompat.ProxyFileDescriptorCallbackCompat
    {
        private readonly Stream _stream;
        private readonly HandlerThread? _handlerThread;

        public ReadWriteCallbacks(Stream stream, HandlerThread? handlerThread = null)
        {
            _stream = stream;
            _handlerThread = handlerThread;
        }

        /// <inheritdoc/>
        public override long OnGetSize()
        {
            return _stream.Length;
        }

        /// <inheritdoc/>
        public override int OnRead(long offset, int size, byte[]? data)
        {
            try
            {
                if (data is null)
                    return 0;

                // Seek to the requested offset
                _stream.Seek(offset, SeekOrigin.Begin);

                // Read the requested data
                return _stream.Read(data.AsSpan(0, size));
            }
            catch (Exception)
            {
                // TODO: Implement more exception handlers
                return 0;
                //throw new ErrnoException(nameof(OnRead), OsConstants.Eio);
            }
        }

        /// <inheritdoc/>
        public override int OnWrite(long offset, int size, byte[]? data)
        {
            try
            {
                if (data is null)
                    return 0;

                // Seek to the requested offset
                _stream.Seek(offset, SeekOrigin.Begin);

                // Write the requested data
                _stream.Write(data.AsSpan(0, size));

                return size;
            }
            catch (Exception ex)
            {
                // TODO: Implement more exception handlers
                _ = ex;
                return 0;
                //throw new ErrnoException(nameof(OnRead), OsConstants.Eio);
            }
        }

        /// <inheritdoc/>
        public override void OnFsync()
        {
            try
            {
                _stream.Flush();
            }
            catch (Exception ex)
            {
                _ = ex;
                // TODO: Implement more exception handlers
                //throw new ErrnoException(nameof(OnRead), OsConstants.Eio);
            }
        }

        /// <inheritdoc/>
        public override void OnRelease()
        {
            _stream.Dispose();
            _handlerThread?.QuitSafely();
        }
    }
}
