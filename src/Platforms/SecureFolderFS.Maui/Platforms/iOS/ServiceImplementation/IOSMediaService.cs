using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class IOSMediaService : BaseMauiMediaService
    {
        /// <inheritdoc/>
        public override async Task<IImageStream> GenerateThumbnailAsync(IFile file, TypeHint typeHint = default, CancellationToken cancellationToken = default)
        {
            if (typeHint == default)
                typeHint = FileTypeHelper.GetTypeHint(file);

            switch (typeHint)
            {
                case TypeHint.Image:
                {
                    await using var stream = await file.OpenReadAsync(cancellationToken).ConfigureAwait(false);
                    return await GenerateImageThumbnailAsync(stream, Constants.Browser.IMAGE_THUMBNAIL_MAX_SIZE).ConfigureAwait(false);
                }

                case TypeHint.Media:
                {
                    await using var stream = await file.OpenReadAsync(cancellationToken).ConfigureAwait(false);
                    return await GenerateVideoThumbnailAsync(stream, TimeSpan.FromSeconds(0)).ConfigureAwait(false);
                }

                default: throw new InvalidOperationException("The provided file type is invalid.");
            }
        }

        private static async Task<IImageStream> GenerateImageThumbnailAsync(Stream stream, uint maxSize)
        {
            using var data = NSData.FromStream(stream);
            using var image = UIImage.LoadFromData(data);
            if (image is null)
                throw new Exception("Failed to load image.");

            // Apply EXIF orientation
            var orientedImage = image.Orientation == UIImageOrientation.Up ? image : UIImage.FromImage(image.CGImage, 1.0f, image.Orientation);

            // Resize
            var scale = Math.Min(maxSize / orientedImage.Size.Width, maxSize / orientedImage.Size.Height);
            var newSize = new CGSize(orientedImage.Size.Width * scale, orientedImage.Size.Height * scale);

            UIGraphics.BeginImageContextWithOptions(newSize, false, 1.0f);
            orientedImage.Draw(new CGRect(CGPoint.Empty, newSize));
            using var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            if (resizedImage is null)
                throw new Exception("Failed to resize image.");

            // Compress to JPEG
            using var jpegData = resizedImage.AsJPEG(Constants.Browser.IMAGE_THUMBNAIL_QUALITY);
            var ms = new OnDemandDisposableStream();
            await jpegData.AsStream().CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;

            return new ImageStream(ms);
        }

        private static async Task<IImageStream> GenerateVideoThumbnailAsync(Stream stream, TimeSpan captureTime)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");
    
            // Write stream to temp file using NSData for speed
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            var data = NSData.FromArray(ms.ToArray());
            data.Save(tempPath, false); // Use NSData's fast file save

            var asset = AVAsset.FromUrl(NSUrl.FromFilename(tempPath));
            var generator = new AVAssetImageGenerator(asset)
            {
                AppliesPreferredTrackTransform = true,
                MaximumSize = new CGSize(320, 240)
            };

            try
            {
                var actualTime = new CMTime((long)captureTime.TotalSeconds, 1);
                var imageRef = generator.CopyCGImageAtTime(actualTime, out var _, out var error);
                if (imageRef is null || error != null)
                    throw new Exception($"Failed to generate thumbnail: {error?.LocalizedDescription}");

                using var image = UIImage.FromImage(imageRef);
                using var jpegData = image.AsJPEG(Constants.Browser.IMAGE_THUMBNAIL_QUALITY);
                var outStream = new OnDemandDisposableStream();
                await jpegData.AsStream().CopyToAsync(outStream).ConfigureAwait(false);
                outStream.Position = 0;

                return new ImageStream(outStream);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

    }
}
