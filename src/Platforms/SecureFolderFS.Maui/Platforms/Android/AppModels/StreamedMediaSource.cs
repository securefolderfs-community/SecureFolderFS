using Android.Media;
using Stream = System.IO.Stream;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="MediaDataSource"/>
    internal sealed class StreamedMediaSource : MediaDataSource
    {
        private readonly Stream _sourceStream;

        /// <inheritdoc/>
        public override long Size => _sourceStream.Length;

        public StreamedMediaSource(Stream sourceStream)
        {
            _sourceStream = sourceStream;
        }

        /// <inheritdoc/>
        public override int ReadAt(long position, byte[]? buffer, int offset, int size)
        {
            if (buffer is null)
                return 0;

            _sourceStream.Position = position;
            return _sourceStream.Read(buffer, offset, size);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            _sourceStream.Dispose();
        }
    }
}
