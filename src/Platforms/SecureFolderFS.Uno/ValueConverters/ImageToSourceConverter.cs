using System;
using System.IO;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Uno.AppModels;

namespace SecureFolderFS.Uno.ValueConverters
{
    public sealed class ImageToSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                ImageBitmap imageBitmap => imageBitmap.Source,
                StreamImageModel imageStream => StreamToImageSource(imageStream.Inner),
                ImageResource resourceImage => new BitmapImage(resourceImage.IsResource
                    ? new Uri($"ms-appx:///{resourceImage.Name}")
                    : new Uri(resourceImage.Name)),

                Uri uri => new BitmapImage(uri),
                _ => null
            };
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private static BitmapImage? StreamToImageSource(Stream? stream)
        {
            if (stream is null)
                return null;

            stream.TrySetPositionOrAdvance(0L);
            var bitmapImage = new BitmapImage();
            var randomAccessStream = stream.AsRandomAccessStream();

            // BitmapImage.SetSourceAsync is async but IValueConverter is sync.
            // Fire-and-forget is intentional here — the image will load asynchronously.
            _ = bitmapImage.SetSourceAsync(randomAccessStream).AsTask().ConfigureAwait(false);

            return bitmapImage;
        }
    }
}
