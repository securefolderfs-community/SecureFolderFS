using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// A simple implementation of <see cref="IImageStream"/> that wraps a <see cref="Stream"/>.
    /// </summary>
    public sealed class StreamImageModel : IImageStream
    {
        private readonly Stream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamImageModel"/> class.
        /// </summary>
        /// <param name="stream">The stream containing image data.</param>
        public StreamImageModel(Stream stream)
        {
            _stream = stream;
        }

        /// <inheritdoc/>
        public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
        {
            var savedPosition = _stream.Position;
            await _stream.CopyToAsync(destination, cancellationToken);

            if (_stream.CanSeek)
                _stream.Position = savedPosition;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}


