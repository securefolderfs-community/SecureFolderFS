using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IImage"/>
    internal sealed class ImageStream : IImage
    {
        private readonly Stream _stream;

        public StreamImageSource Source { get; }

        public ImageStream(Stream stream)
        {
            _stream = stream;
            Source = new();
            Source.Stream = (cancellationToken) => Task.FromResult(stream);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
