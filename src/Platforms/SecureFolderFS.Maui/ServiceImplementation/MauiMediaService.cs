using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI;
using SkiaSharp;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;
using Stream = System.IO.Stream;

#if ANDROID
using Android.Media;
#endif

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class MauiMediaService : IMediaService
    {
        /// <inheritdoc/>
        public async Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken)
        {
            var stream = await file.OpenReadAsync(cancellationToken);
            return new ImageStream(stream);
        }

        /// <inheritdoc/>
        public async Task<IImageStream?> GenerateThumbnailAsync(IFile file, CancellationToken cancellationToken = default)
        {
            var typeHint = FileTypeHelper.GetType(file);
            switch (typeHint)
            {
                case TypeHint.Image:
                {
                    await using var sourceStream = await file.OpenReadAsync(cancellationToken);
                    return ResizeImage(sourceStream, Constants.Application.IMAGE_THUMBNAIL_MAX_SIZE);
                }

                case TypeHint.Media:
                {
#if ANDROID
                    await using var sourceStream = await file.OpenReadAsync(cancellationToken);
                    return Android_ExtractFrame(sourceStream, TimeSpan.FromSeconds(0));
#elif IOS
                    IOS_ExtractFrame();
#endif
                    
                    break;
                }
            }
            
            return null;
        }

        /// <inheritdoc/>
        public async Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken)
        {
            var classification = FileTypeHelper.GetClassification(file);
            var stream = await file.OpenReadAsync(cancellationToken);

            return new VideoStreamServer(stream, classification.MimeType);
        }
        
#if ANDROID
        private static ImageStream? Android_ExtractFrame(Stream stream, TimeSpan captureTime)
        {
            using var retriever = new MediaMetadataRetriever();
            retriever.SetDataSource(new StreamedMediaSource(stream));
            
            var frameBitmap = retriever.GetFrameAtTime(captureTime.Ticks, Option.Closest);
            if (frameBitmap is null)
                return null;

            using var frameStream = new MemoryStream();
            frameBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg!, Constants.Application.VIDEO_THUMBNAIL_QUALITY, frameStream);
            frameStream.Position = 0L;

            return ResizeImage(frameStream, Constants.Application.IMAGE_THUMBNAIL_MAX_SIZE);
        }
#endif

#if IOS || MACCATALYST
    private static void IOS_ExtractFrame(string videoPath, string outputPath, TimeSpan captureTime)
    {
        var asset = new AVAsset(NSUrl.FromFilename(videoPath));
        var generator = new AVAssetImageGenerator(asset) { AppliesPreferredTrackTransform = true };
        var time = CMTime.FromSeconds(captureTime.TotalSeconds, 1);

        NSError error;
        var imageRef = generator.CopyCGImageAtTime(time, out error);
        if (imageRef != null)
        {
            using var image = new UIImage(imageRef);
            using var data = image.AsJPEG(0.8f);
            File.WriteAllBytes(outputPath, data.ToArray());
        }
    }
#endif
        
        private static ImageStream ResizeImage(Stream sourceStream, uint maxSize)
        {
            using var original = SKBitmap.Decode(sourceStream);
            if (original is null)
                throw new BadImageFormatException("Failed to load image.");

            // Calculate new dimensions while maintaining aspect ratio
            var scale = Math.Min((float)maxSize / original.Width, (float)maxSize / original.Height);
            var newWidth = (int)(original.Width * scale);
            var newHeight = (int)(original.Height * scale);

            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (resized is null)
                throw new Exception("Failed to resize image.");

            using var image = SKImage.FromBitmap(resized);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, UI.Constants.Application.IMAGE_THUMBNAIL_QUALITY);
            var destinationStream = new OnDemandDisposableStream();
            encoded.SaveTo(destinationStream);

            destinationStream.Position = 0L;
            return new ImageStream(destinationStream);
        }
    }
}
