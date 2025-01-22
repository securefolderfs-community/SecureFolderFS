using System;
using Windows.Graphics.Imaging;

namespace SecureFolderFS.Uno.Helpers
{
    internal static class MimeHelpers
    {
        public static Guid MimeToBitmapDecoder(string mimeType)
        {
            return mimeType switch
            {
                "image/bmp" => BitmapDecoder.BmpDecoderId,
                "image/gif" => BitmapDecoder.GifDecoderId,
                "image/heic" => BitmapDecoder.HeifDecoderId,
                "image/vnd.microsoft.icon" => BitmapDecoder.IcoDecoderId,
                "image/jpeg" => BitmapDecoder.JpegDecoderId,
                "image/png" => BitmapDecoder.PngDecoderId,
                "image/tiff" => BitmapDecoder.TiffDecoderId,
                "image/webp" => BitmapDecoder.WebpDecoderId,

                _ => BitmapDecoder.PngDecoderId
            };
        }

        public static Guid MimeToBitmapEncoder(string mimeType)
        {
            return mimeType switch
            {
                "image/bmp" => BitmapEncoder.BmpEncoderId,
                "image/gif" => BitmapEncoder.GifEncoderId,
                "image/heic" => BitmapEncoder.HeifEncoderId,
                "image/vnd.microsoft.icon" => BitmapEncoder.PngEncoderId,
                "image/jpeg" => BitmapEncoder.JpegEncoderId,
                "image/png" => BitmapEncoder.PngEncoderId,
                "image/tiff" => BitmapEncoder.TiffEncoderId,
                "image/webp" => BitmapEncoder.PngEncoderId,

                _ => BitmapEncoder.PngEncoderId
            };
        }
    }
}
