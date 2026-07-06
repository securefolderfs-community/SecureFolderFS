using Android.Media;
using Stream = System.IO.Stream;

namespace SecureFolderFS.Core.MobileFS.AppModels
{
    /// <inheritdoc cref="MediaDataSource"/>
    internal sealed class StreamedMediaSource : MediaDataSource
    {
        private readonly Stream _sourceStream;

        /// <inheritdoc/>
        public override long Size
        {
            get
            {
                try
                {
                    // The contract specifies -1 when the size is unknown
                    return _sourceStream.CanSeek ? _sourceStream.Length : -1L;
                }
                catch (Exception)
                {
                    return -1L;
                }
            }
        }

        public StreamedMediaSource(Stream sourceStream)
        {
            _sourceStream = sourceStream;
        }

        /// <inheritdoc/>
        public override int ReadAt(long position, byte[]? buffer, int offset, int size)
        {
            try
            {
                if (buffer is null || size <= 0)
                    return 0;

                _sourceStream.Position = position;

                // The contract specifies -1 for the end of the stream. Returning 0 would
                // indicate that no data was read yet, causing the extractor to retry forever
                var read = _sourceStream.Read(buffer, offset, size);
                return read == 0 ? -1 : read;
            }
            catch (Exception)
            {
                // Errors must not propagate across the JNI boundary
                return -1;
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            _sourceStream.Dispose();
        }
    }
}
