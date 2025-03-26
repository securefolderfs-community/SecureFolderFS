using SecureFolderFS.Shared.ComponentModel;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IImage"/>
    internal sealed class ImageStream : IImageStream
    {
        public Stream Stream { get; }

        public StreamImageSource Source { get; }

        public ImageStream(Stream stream)
        {
            Stream = stream;
            Source = new();
            Source.Stream = (ct) => Task.FromResult(stream);
        }
        
        /// <inheritdoc/>
        public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
        {
            var savedPosition = Stream.Position;
            await Stream.CopyToAsync(destination, cancellationToken);
            Stream.Position = savedPosition;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Stream is OnDemandDisposableStream onDemandDisposableStream)
                onDemandDisposableStream.ForceClose();
            else
                Stream.Dispose();
        }
    }
}
