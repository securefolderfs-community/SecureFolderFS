// Some parts of the following code were used from Avalonia (AvaloniaUI) on the MIT License basis.
/*
   The MIT License (MIT)
   
   Copyright (c) AvaloniaUI OÜ All Rights Reserved
   
   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using Foundation;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage
{
    /// <inheritdoc cref="Stream"/>
    internal sealed class IOSSecurityScopedStream : Stream
    {
        private readonly NSUrl _url;
        private readonly FileStream _stream;
        private readonly UIDocument _document;
        private readonly NSUrl _securityScopedAncestorUrl;

        internal IOSSecurityScopedStream(NSUrl url, NSUrl securityScopedAncestorUrl, FileAccess access, FileShare share = FileShare.None)
        {
            _document = new UIDocument(url);
            var path = _document.FileUrl.Path!;
            _url = url;
            _securityScopedAncestorUrl = securityScopedAncestorUrl;
            _securityScopedAncestorUrl.StartAccessingSecurityScopedResource();
            _stream = File.Open(path, FileMode.Open, access, share);
        }

        /// <inheritdoc/>
        public override bool CanRead => _stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => _stream.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        /// <inheritdoc/>
        public override void Flush() =>
            _stream.Flush();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) =>
            _stream.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) =>
            _stream.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value) =>
            _stream.SetLength(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) =>
            _stream.Write(buffer, offset, count);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _stream.Dispose();
                _document.Dispose();
                _securityScopedAncestorUrl.StopAccessingSecurityScopedResource();
            }
        }
    }
}
