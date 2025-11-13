using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IImage"/>
    internal sealed class ImageRemoteUrl : IImage
    {
        /// <summary>
        /// Gets the URL of the image.
        /// </summary>
        public string Url { get; }

        public ImageRemoteUrl(string url)
        {
            Url = url;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
