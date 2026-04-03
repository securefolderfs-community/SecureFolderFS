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
