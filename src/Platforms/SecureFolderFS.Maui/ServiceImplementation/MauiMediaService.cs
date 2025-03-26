using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI;
using SkiaSharp;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

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
        public async Task<IImageStream> GenerateThumbnailAsync(IFile file, CancellationToken cancellationToken = default)
        {
            await using var sourceStream = await file.OpenReadAsync(cancellationToken);
            using var original = SKBitmap.Decode(sourceStream);

            if (original is null)
                throw new BadImageFormatException("Failed to load image.");

            // Calculate new dimensions while maintaining aspect ratio
            var scale = Math.Min((float)UI.Constants.Application.IMAGE_THUMBNAIL_MAX_SIZE / original.Width, (float)UI.Constants.Application.IMAGE_THUMBNAIL_MAX_SIZE / original.Height);
            var newWidth = (int)(original.Width * scale);
            var newHeight = (int)(original.Height * scale);

            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (resized is null)
                throw new Exception("Failed to resize image.");

            using var image = SKImage.FromBitmap(resized);
            using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, UI.Constants.Application.IMAGE_THUMBNAIL_QUALITY);
            var destinationStream = new OnDemandDisposableStream();
            encoded.SaveTo(destinationStream);

            destinationStream.Position = 0L;
            return new ImageStream(destinationStream);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken)
        {
            var classification = FileTypeHelper.GetClassification(file);
            var stream = await file.OpenReadAsync(cancellationToken);

            return new VideoStreamServer(stream, classification.MimeType);
        }
    }
}
