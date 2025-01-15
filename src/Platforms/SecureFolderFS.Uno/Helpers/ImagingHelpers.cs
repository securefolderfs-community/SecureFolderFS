using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Uno.AppModels;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace SecureFolderFS.Uno.Helpers
{
    internal static class ImagingHelpers
    {
        public static async Task<IImage> GetBitmapFromStreamAsync(IRandomAccessStream winrtStream, string mimeType, CancellationToken cancellationToken)
        {
            using var winrtMemStream = new InMemoryRandomAccessStream();

            var decoderGuid = MimeHelpers.MimeToBitmapDecoder(mimeType);
            var encoderGuid = MimeHelpers.MimeToBitmapEncoder(mimeType);

            var decoder = await BitmapDecoder.CreateAsync(decoderGuid, winrtStream);
            var encoder = await BitmapEncoder.CreateAsync(encoderGuid, winrtMemStream);

            var softwareBitmap = await decoder.GetSoftwareBitmapAsync().AsTask(cancellationToken);
            encoder.SetSoftwareBitmap(softwareBitmap);

            await encoder.FlushAsync().AsTask(cancellationToken);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(winrtMemStream).AsTask(cancellationToken);

            return new ImageBitmap(bitmap, softwareBitmap);
        }
    }
}
