using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using ImageIO;
using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Streams;
using SecureFolderFS.UI;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class IOSMediaService : BaseMauiMediaService
    {
        /// <inheritdoc/>
        public override async Task<IImageStream> GenerateThumbnailAsync(IFile file, TypeHint typeHint = default,
            CancellationToken cancellationToken = default)
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

                    var extension = Path.GetExtension(file.Name);
                    return await GenerateVideoThumbnailAsync(stream, extension, TimeSpan.FromSeconds(0)).ConfigureAwait(false);
                }

                default: throw new InvalidOperationException("The provided file type is invalid.");
            }
        }

        private static async Task<IImageStream> GenerateImageThumbnailAsync(Stream stream, uint maxSize)
        {
            using var data = NSData.FromStream(stream);
            if (data is null)
                throw new Exception("Failed to load image data.");

            using var source = CGImageSource.FromData(data);
            if (source is null)
                throw new Exception("Failed to create image source.");

            var options = new CGImageThumbnailOptions
            {
                MaxPixelSize = (int)maxSize,
                ShouldAllowFloat = false,
                CreateThumbnailWithTransform = true, // handles EXIF orientation
                CreateThumbnailFromImageAlways = true
            };

            using var cgImage = source.CreateThumbnail(0, options);
            if (cgImage is null)
                throw new Exception("Failed to create thumbnail.");

            using var image = UIImage.FromImage(cgImage);
            using var jpegData = image.AsJPEG(Constants.Browser.IMAGE_THUMBNAIL_QUALITY);
            if (jpegData is null)
                throw new FormatException("Failed to convert image to JPEG.");

            var memoryStream = new MemoryStream((int)jpegData.Length);
            await jpegData.AsStream().CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0L;

            return new ImageStreamSource(new NonDisposableStream(memoryStream));
        }

        private static async Task<IImageStream> GenerateVideoThumbnailAsync(Stream stream, string extension, TimeSpan captureTime)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
            try
            {
                // Only read up to a limited prefix of the file. AVFoundation can typically
                // decode the first keyframe from the first few MBs of a well-formed MP4,
                // since moov/stbl metadata is usually at the front.
                const int maxPrefixBytes = 4 * 1024 * 1024; // 4 MB
                await using (var fileStream = File.OpenWrite(tempPath))
                {
                    var buffer = new byte[81920];
                    int totalRead = 0, read;
                    while (totalRead < maxPrefixBytes &&
                           (read = await stream
                                .ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, maxPrefixBytes - totalRead)))
                               .ConfigureAwait(false)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                        totalRead += read;
                    }
                }

                var asset = AVAsset.FromUrl(NSUrl.FromFilename(tempPath));
                var generator = new AVAssetImageGenerator(asset)
                {
                    AppliesPreferredTrackTransform = true,
                    MaximumSize = new CGSize(320, 240)
                };

                var actualTime = new CMTime((long)captureTime.TotalSeconds, 1);
                var tcs = new TaskCompletionSource<CGImage>(TaskCreationOptions.RunContinuationsAsynchronously);
                var times = new[] { NSValue.FromCMTime(actualTime) };

                generator.GenerateCGImagesAsynchronously(times, (_, image, _, _, error) =>
                {
                    if (error != null || image is null)
                        tcs.TrySetException(new FormatException($"Failed to generate thumbnail: {error?.LocalizedDescription}"));
                    else
                        tcs.TrySetResult(image);
                });

                using var imageRef = await tcs.Task.ConfigureAwait(false);
                using var image = UIImage.FromImage(imageRef);
                using var jpegData = image.AsJPEG(Constants.Browser.IMAGE_THUMBNAIL_QUALITY);
                if (jpegData is null)
                    throw new FormatException("Failed to convert image to JPEG.");

                var memoryStream = new MemoryStream();
                await jpegData.AsStream().CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0L;

                return new ImageStreamSource(new NonDisposableStream(memoryStream));
            }
            finally
            {
                File.Delete(tempPath);
            }
        }
    }
}
