using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// A simple implementation of <see cref="IImageStream"/> that wraps a <see cref="System.IO.Stream"/>.
    /// </summary>
    public sealed class StreamImageModel : IImageStream
    {
        /// <summary>
        /// Gets the stream containing image data.
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamImageModel"/> class.
        /// </summary>
        /// <param name="stream">The stream containing image data.</param>
        public StreamImageModel(Stream stream)
        {
            Stream = stream;
        }

        /// <inheritdoc/>
        public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
        {
            var savedPosition = Stream.Position;
            await Stream.CopyToAsync(destination, cancellationToken);

            if (Stream.CanSeek)
                Stream.Position = savedPosition;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}


