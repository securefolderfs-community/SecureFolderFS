using Android.Graphics;
using Android.Media;
using SecureFolderFS.Storage.Streams;
using ExifInterface = AndroidX.ExifInterface.Media.ExifInterface;
using Stream = System.IO.Stream;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.Helpers
{
    public static class ThumbnailHelpers
    {
        public static async Task<Stream> GenerateImageThumbnailAsync(Stream stream, uint maxSize)
        {
            // Read EXIF
            var exif = new ExifInterface(stream);
            stream.Position = 0;

            // Attempt to get dimensions from EXIF tags to avoid a full bounds decode pass
            var exifWidth = exif.GetAttributeInt(ExifInterface.TagImageWidth, 0);
            var exifHeight = exif.GetAttributeInt(ExifInterface.TagImageLength, 0);

            int width, height;
            if (exifWidth > 0 && exifHeight > 0)
            {
                width = exifWidth;
                height = exifHeight;
            }
            else
            {
                // Fall back to bounds decode if EXIF dimensions are missing
                var boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true };
                await BitmapFactory.DecodeStreamAsync(stream, null, boundsOptions).ConfigureAwait(false);
                stream.Position = 0L;

                width = boundsOptions.OutWidth;
                height = boundsOptions.OutHeight;
            }

            var inSampleSize = CalculateInSampleSize(width, height, (int)maxSize);

            var options = new BitmapFactory.Options
            {
                InJustDecodeBounds = false,
                InSampleSize = inSampleSize
            };

            using var bitmap = await BitmapFactory.DecodeStreamAsync(stream, null, options).ConfigureAwait(false);
            if (bitmap is null)
                throw new Exception("Failed to decode image.");

            using var rotated = ApplyExifOrientation(bitmap, exif);
            return await CompressBitmapAsync(rotated).ConfigureAwait(false);
        }

        private static int CalculateInSampleSize(int width, int height, int reqSize)
        {
            var inSampleSize = 1;
            while ((width / (inSampleSize * 2)) >= reqSize && (height / (inSampleSize * 2)) >= reqSize)
                inSampleSize *= 2;

            return inSampleSize;
        }

        private static Bitmap ApplyExifOrientation(Bitmap bitmap, ExifInterface exif)
        {
            var orientation = (Orientation)exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);
            var matrix = new Matrix();

            switch (orientation)
            {
                case Orientation.Normal:
                    return bitmap;

                case Orientation.FlipHorizontal:
                    matrix.SetScale(-1, 1);
                    break;

                case Orientation.Rotate180:
                    matrix.SetRotate(180);
                    break;

                case Orientation.FlipVertical:
                    matrix.SetScale(1, -1);
                    break;

                case Orientation.Transpose:
                    matrix.SetRotate(-90);
                    matrix.PostScale(1, -1);
                    break;

                case Orientation.Rotate90:
                    matrix.SetRotate(90);
                    break;

                case Orientation.Transverse:
                    matrix.SetRotate(90);
                    matrix.PostScale(-1, 1);
                    break;

                case Orientation.Rotate270:
                    matrix.SetRotate(-90);
                    break;

                default:
                    return bitmap;
            }

            var rotated = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
            bitmap.Dispose();
            return rotated;
        }

        public static async Task<Stream> CompressBitmapAsync(Bitmap bitmap)
        {
            const int IMAGE_THUMBNAIL_QUALITY = 70;

            var memoryStream = new MemoryStream();
            await bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg!, IMAGE_THUMBNAIL_QUALITY, memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0L;
            bitmap.Dispose();

            return new NonDisposableStream(memoryStream);
        }
    }
}