using Android.Runtime;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.Streams
{
    internal sealed class InputOutputStream : Stream
    {
        private readonly InputStreamInvoker _inputStream;
        private readonly OutputStreamInvoker _outputStream;

        /// <inheritdoc/>
        public override bool CanRead => _inputStream.CanRead;
        
        /// <inheritdoc/>
        public override bool CanSeek => _inputStream.CanSeek;
        
        /// <inheritdoc/>
        public override bool CanWrite => _outputStream.CanWrite;
        
        /// <inheritdoc/>
        public override long Length => _inputStream.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _inputStream.Position;
            set => _inputStream.Position = value;
        }

        public InputOutputStream(InputStreamInvoker inputStream, OutputStreamInvoker outputStream)
        {
            _inputStream = inputStream;
            _outputStream = outputStream;
        }
        
        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inputStream.Read(buffer, offset, count);
        }
        
        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var sL = _inputStream.Length;
            var sP = _inputStream.Position;
            
            _outputStream.Write(buffer, offset, count);

            var eL = _inputStream.Length;
            var eP = _inputStream.Position;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            
        }
        
        /// <inheritdoc/>
        public override void Flush()
        {
            _outputStream.Flush();
        }
    }
}
