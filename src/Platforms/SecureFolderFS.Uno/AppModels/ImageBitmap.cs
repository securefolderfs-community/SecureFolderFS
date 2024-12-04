using Microsoft.UI.Xaml.Media.Imaging;
using SecureFolderFS.Shared.ComponentModel;
using Windows.Graphics.Imaging;

namespace SecureFolderFS.Uno.AppModels
{
    /// <inheritdoc cref="IImage"/>
    public sealed class ImageBitmap(BitmapImage source, SoftwareBitmap? softwareBitmap) : IImage
    {
        public BitmapImage Source { get; } = source;

        public SoftwareBitmap? SoftwareBitmap { get; } = softwareBitmap;

        /// <inheritdoc/>
        public void Dispose()
        {
            SoftwareBitmap?.Dispose();
        }
    }
}
