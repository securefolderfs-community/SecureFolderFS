using Android.Graphics;
using Android.Media;
using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI;
using SkiaSharp;
using ExifInterface = AndroidX.ExifInterface.Media.ExifInterface;
using Stream = System.IO.Stream;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class AndroidMediaService : BaseMauiMediaService
    {
        /// <inheritdoc/>
        public override async Task<IImageStream?> GenerateThumbnailAsync(IFile file,
            CancellationToken cancellationToken = default)
        {
            var typeHint = FileTypeHelper.GetType(file);
            switch (typeHint)
            {
                case TypeHint.Image:
                {
                    await using var sourceStream = await file.OpenReadAsync(cancellationToken);
                    return ResizeImage(sourceStream, Constants.Browser.IMAGE_THUMBNAIL_MAX_SIZE);
                }

                case TypeHint.Media:
                {
                    await using var sourceStream = await file.OpenReadAsync(cancellationToken);
                    return Android_ExtractFrame(sourceStream, TimeSpan.FromSeconds(0));
                }
            }

            return null;
        }

        private static ImageStream ResizeImage(Stream sourceStream, uint maxSize)
        {
#if ANDROID
            // Read EXIF orientation
            var exif = new ExifInterface(sourceStream);
            var orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Undefined);
            sourceStream.Position = 0; // Reset stream
#endif

            using var original = SKBitmap.Decode(sourceStream);
            if (original is null)
                throw new BadImageFormatException("Failed to load image.");

#if ANDROID
            using var rotated = ApplyExifOrientation(original, orientation);
#else
            using var rotated = original;
#endif

            // Resize with aspect ratio
            var scale = Math.Min((float)maxSize / rotated.Width, (float)maxSize / rotated.Height);
            var newWidth = (int)(rotated.Width * scale);
            var newHeight = (int)(rotated.Height * scale);

            using var resized = rotated.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (resized is null)
                throw new Exception("Failed to resize image.");

            using var image = SKImage.FromBitmap(resized);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, Constants.Browser.IMAGE_THUMBNAIL_QUALITY);
            var destinationStream = new OnDemandDisposableStream();
            encoded.SaveTo(destinationStream);

            destinationStream.Position = 0L;
            return new ImageStream(destinationStream);
        }

#if ANDROID
        private static SKBitmap ApplyExifOrientation(SKBitmap bitmap, int orientation)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            SKBitmap result;
            switch (orientation)
            {
                // 1
                case (int)Orientation.Normal: return bitmap;

                // 2
                case (int)Orientation.FlipHorizontal:
                {
                    result = new SKBitmap(width, height);
                    using var canvas = new SKCanvas(result);
                    
                    canvas.Scale(-1, 1);
                    canvas.Translate(-width, 0);
                    canvas.DrawBitmap(bitmap, 0, 0);
                    break;
                }

                // 3
                case (int)Orientation.Rotate180:
                {
                    result = new SKBitmap(width, height);
                    using var canvas = new SKCanvas(result);
                 
                    canvas.RotateDegrees(180, width / 2f, height / 2f);
                    canvas.DrawBitmap(bitmap, 0, 0);
                    break;
                }

                // 4
                case (int)Orientation.FlipVertical:
                {
                    result = new SKBitmap(width, height);
                    using var canvas = new SKCanvas(result);
                 
                    canvas.Scale(1, -1);
                    canvas.Translate(0, -height);
                    canvas.DrawBitmap(bitmap, 0, 0);
                    break;
                }

                // 5
                case (int)Orientation.Transpose:
                {
                    result = new SKBitmap(height, width);
                    using var canvas = new SKCanvas(result);
                 
                    canvas.RotateDegrees(90);
                    canvas.Scale(1, -1);
                    canvas.DrawBitmap(bitmap, 0, 0);
                    break;
                }
                
                // 6
                case (int)Orientation.Rotate90:
                {
                    result = new SKBitmap(height, width);
                    using var canvas = new SKCanvas(result);
                    
                    canvas.Translate(height, 0);
                    canvas.RotateDegrees(90);
                    canvas.DrawBitmap(bitmap, 0, 0);
                    break;
                }

                // 7
                case (int)Orientation.Transverse:
                {
                    result = new SKBitmap(height, width);
                    using var canvas = new SKCanvas(result);
                 
                    canvas.Translate(0, width);
                    canvas.RotateDegrees(270);
                    canvas.Scale(1, -1);
                    canvas.DrawBitmap(bitmap, 0, 0);
                    break;
                }
                
                // 8
                case (int)Orientation.Rotate270:
                {
                    result = new SKBitmap(height, width);
                    using var canvas = new SKCanvas(result);
                 
                    canvas.Translate(0, width);
                    canvas.RotateDegrees(270);
                    canvas.DrawBitmap(bitmap, 0, 0);
                    break;
                }

                default: return bitmap;
            }

            return result;
        }
#endif
        
        private static ImageStream? Android_ExtractFrame(Stream stream, TimeSpan captureTime)
        {
            using var retriever = new MediaMetadataRetriever();
            retriever.SetDataSource(new StreamedMediaSource(stream));

            var frameBitmap = retriever.GetFrameAtTime(captureTime.Ticks, Option.Closest);
            if (frameBitmap is null)
                return null;

            using var frameStream = new MemoryStream();
            frameBitmap.Compress(Bitmap.CompressFormat.Jpeg!, Constants.Browser.VIDEO_THUMBNAIL_QUALITY, frameStream);
            frameStream.Position = 0L;

            return ResizeImage(frameStream, Constants.Browser.IMAGE_THUMBNAIL_MAX_SIZE);
        }
    }
}
