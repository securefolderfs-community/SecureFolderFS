using Android.OS;
using Android.Systems;

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
            try
            {
                return _stream.Length;
            }
            catch (Exception)
            {
                throw new ErrnoException(nameof(OnGetSize), OsConstants.Eio);
            }
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
                // Returning 0 would signal EOF and silently truncate the read
                throw new ErrnoException(nameof(OnRead), OsConstants.Eio);
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
            catch (Exception)
            {
                // Returning 0 would signal a successful zero-byte write and silently lose data
                throw new ErrnoException(nameof(OnWrite), OsConstants.Eio);
            }
        }

        /// <inheritdoc/>
        public override void OnFsync()
        {
            try
            {
                if (_stream.CanWrite)
                    _stream.Flush();
            }
            catch (Exception)
            {
                throw new ErrnoException(nameof(OnFsync), OsConstants.Eio);
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
