using Android.Graphics;
using Android.Media;
using SecureFolderFS.Core.MobileFS.AppModels;
using SecureFolderFS.Storage.Streams;
using ExifInterface = AndroidX.ExifInterface.Media.ExifInterface;
using Stream = System.IO.Stream;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.Helpers
{
    public static class ThumbnailHelpers
    {
        private const int IMAGE_THUMBNAIL_QUALITY = 70;

        /// <summary>
        /// Generates a scaled-down thumbnail from the provided image file stream with a maximum size constraint.
        /// </summary>
        /// <param name="stream">The image file stream from which the thumbnail is generated.</param>
        /// <param name="maxSize">The maximum size (width or height) that the thumbnail should not exceed.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a stream containing the compressed thumbnail image.</returns>
        public static async Task<Stream> GenerateImageThumbnailAsync(Stream stream, uint maxSize)
        {
            // Read EXIF
            var exif = new ExifInterface(stream);
            stream.Position = 0L;

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
            var options = new BitmapFactory.Options()
            {
                InJustDecodeBounds = false,
                InSampleSize = inSampleSize
            };

            using var bitmap = await BitmapFactory.DecodeStreamAsync(stream, null, options).ConfigureAwait(false);
            if (bitmap is null)
                throw new Exception("Failed to decode image.");

            // ApplyExifOrientation returns the same instance when no transformation is
            // needed - only dispose the result when a new bitmap was actually created
            var oriented = ApplyExifOrientation(bitmap, exif);
            try
            {
                return await CompressBitmapAsync(oriented).ConfigureAwait(false);
            }
            finally
            {
                if (!ReferenceEquals(oriented, bitmap))
                    oriented.Dispose();
            }
        }

        /// <summary>
        /// Generates a video thumbnail as a compressed stream by capturing a frame at the specified timestamp.
        /// </summary>
        /// <param name="stream">The video file stream from which the thumbnail is generated.</param>
        /// <param name="captureTime">The time position in the video to capture the thumbnail frame.</param>
        /// <param name="width">The width of the thumbnail image.</param>
        /// <param name="height">The height of the thumbnail image.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a stream containing the compressed thumbnail image.</returns>
        public static async Task<Stream> GenerateVideoThumbnailAsync(Stream stream, TimeSpan captureTime, int width = 320, int height = 240)
        {
            using var retriever = new MediaMetadataRetriever();
            await retriever.SetDataSourceAsync(new StreamedMediaSource(stream)).ConfigureAwait(false);

            // The retriever expects the timestamp in microseconds, not in ticks (100ns units).
            // ClosestSync clamps to the nearest sync frame, so a capture time past the end is safe
            var captureTimeUs = captureTime.Ticks / TimeSpan.TicksPerMicrosecond;

            // Use scaled frame for efficiency
            using var bitmap = retriever.GetScaledFrameAtTime(captureTimeUs, Option.ClosestSync, width, height);
            if (bitmap is null)
                throw new NotSupportedException("Could not retrieve scaled frame.");

            return await CompressBitmapAsync(bitmap).ConfigureAwait(false);
        }

        private static int CalculateInSampleSize(int width, int height, int reqSize)
        {
            var inSampleSize = 1;
            while ((width / (inSampleSize * 2)) >= reqSize && (height / (inSampleSize * 2)) >= reqSize)
                inSampleSize *= 2;

            return inSampleSize;
        }

        /// <summary>
        /// Applies the EXIF orientation to <paramref name="bitmap"/>.
        /// </summary>
        /// <remarks>
        /// The caller retains ownership of <paramref name="bitmap"/>. When a transformation is
        /// applied, a new bitmap (owned by the caller) is returned; otherwise the same instance.
        /// </remarks>
        private static Bitmap ApplyExifOrientation(Bitmap bitmap, ExifInterface exif)
        {
            var orientation = (Orientation)exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);
            using var matrix = new Matrix();

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

            return Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
        }

        /// <summary>
        /// Compresses <paramref name="bitmap"/> into an image stream. The caller retains ownership of the bitmap.
        /// </summary>
        private static async Task<Stream> CompressBitmapAsync(Bitmap bitmap)
        {
            var format = bitmap.HasAlpha ? Bitmap.CompressFormat.Png! : Bitmap.CompressFormat.Jpeg!;
            var memoryStream = new MemoryStream();
            await bitmap.CompressAsync(format, IMAGE_THUMBNAIL_QUALITY, memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0L;

            return new NonDisposableStream(memoryStream);
        }
    }
}