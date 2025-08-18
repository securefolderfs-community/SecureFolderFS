using Android.Graphics;
using Android.Media;
using SecureFolderFS.Shared.Models;
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

            // Get bounds
            var boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true };
            await BitmapFactory.DecodeStreamAsync(stream, null, boundsOptions).ConfigureAwait(false);
            stream.Position = 0;

            var (width, height) = (boundsOptions.OutWidth, boundsOptions.OutHeight);
            var scale = Math.Min((float)maxSize / width, (float)maxSize / height);
            var inSampleSize = CalculateInSampleSize(width, height, (int)(width * scale), (int)(height * scale));

            var options = new BitmapFactory.Options
            {
                InJustDecodeBounds = false,
                InSampleSize = inSampleSize
            };

            using var bitmap = await BitmapFactory.DecodeStreamAsync(stream, null, options);
            if (bitmap is null)
                throw new Exception("Failed to decode image.");

            using var rotated = ApplyExifOrientation(bitmap, exif);
            return await CompressBitmapAsync(rotated).ConfigureAwait(false);
        }

        private static int CalculateInSampleSize(int width, int height, int reqWidth, int reqHeight)
        {
            var inSampleSize = 1;
            if (height <= reqHeight && width <= reqWidth)
                return inSampleSize;

            var halfHeight = height / 2;
            var halfWidth = width / 2;

            while ((halfHeight / inSampleSize) >= reqHeight && (halfWidth / inSampleSize) >= reqWidth)
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
            bitmap.Recycle();
            bitmap.Dispose();
            return rotated;
        }

        public static async Task<Stream> CompressBitmapAsync(Bitmap bitmap)
        {
            const int IMAGE_THUMBNAIL_QUALITY = 80;
            var ms = new OnDemandDisposableStream();
            await bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, IMAGE_THUMBNAIL_QUALITY, ms).ConfigureAwait(false);
            ms.Position = 0;
            bitmap.Recycle();
            return ms;
        }
    }
}
