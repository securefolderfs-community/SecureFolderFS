using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Streams;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IImageStream"/>
    internal sealed class ImageStreamSource : IImageStream
    {
        /// <summary>
        /// Gets the streamed image source.
        /// </summary>
        public StreamImageSource Source { get; }

        /// <inheritdoc/>
        public Stream Inner { get; }

        public ImageStreamSource(Stream inner)
        {
            Inner = inner;
            Source = new StreamImageSource
            {
                Stream = _ =>
                {
                    Inner.TrySetPositionOrAdvance(0L);
                    return Task.FromResult(Inner);
                }
            };
        }

        public ImageStreamSource(byte[] imageData)
        {
            // Serve a fresh stream per request - the platform image loader disposes the
            // stream after decoding and may request it again (recycled views, re-layouts).
            // A single shared stream would be dead by the time a second request arrives
            Inner = new MemoryStream(imageData, writable: false);
            Source = new StreamImageSource
            {
                Stream = _ => Task.FromResult<Stream>(new MemoryStream(imageData, writable: false))
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Inner is NonDisposableStream nonDisposableStream)
                nonDisposableStream.ForceClose();
            else
                Inner.Dispose();
        }
    }
}
