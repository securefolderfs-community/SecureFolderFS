using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using ImageIO;
using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.Platforms.iOS.AppModels;
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
                    // Capture at one second rather than the very first frame, which is often
                    // black (fade-ins). The generator clamps the position for shorter videos
                    await using var stream = await file.OpenReadAsync(cancellationToken).ConfigureAwait(false);

                    var extension = Path.GetExtension(file.Name);
                    return await GenerateVideoThumbnailAsync(stream, extension, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
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
            using var encodedData = EncodeImage(image, cgImage.AlphaInfo);
            if (encodedData is null)
                throw new FormatException("Failed to encode the thumbnail.");

            return await ToImageStreamAsync(encodedData).ConfigureAwait(false);
        }

        private static async Task<IImageStream> GenerateVideoThumbnailAsync(Stream stream, string extension, TimeSpan captureTime)
        {
            // Resolve the UTI of the container format for the resource loader
            var normalizedExtension = extension.TrimStart('.').ToLowerInvariant();
            var contentType = normalizedExtension switch
            {
                "mp4" or "m4v" => "public.mpeg-4",
                "mov" or "qt" => "com.apple.quicktime-movie",
                "mpg" or "mpeg" => "public.mpeg",
                "avi" => "public.avi",
                "3gp" => "public.3gpp",
                _ => "public.movie"
            };

            // A custom URL scheme forces AVFoundation to consult the resource loader delegate,
            // which serves byte ranges directly from the (decrypted) stream. This avoids writing
            // plaintext to a temporary file and supports videos with metadata at the end of the file
            var url = NSUrl.FromString($"securefolderfs-stream://thumbnail/video.{normalizedExtension}");
            if (url is null)
                throw new FormatException("Failed to create the resource URL.");

            using var loaderQueue = new DispatchQueue($"{nameof(SecureFolderFS)}.{nameof(Maui)}.ResourceLoader");
            using var loaderDelegate = new StreamedResourceLoaderDelegate(stream, contentType);
            using var asset = AVUrlAsset.Create(url);
            asset.ResourceLoader.SetDelegate(loaderDelegate, loaderQueue);

            using var generator = new AVAssetImageGenerator(asset);
            generator.AppliesPreferredTrackTransform = true;
            generator.MaximumSize = new CGSize(320, 320);

            var tcs = new TaskCompletionSource<CGImage>(TaskCreationOptions.RunContinuationsAsynchronously);
            var times = new[] { NSValue.FromCMTime(CMTime.FromSeconds(captureTime.TotalSeconds, 600)) };

            generator.GenerateCGImagesAsynchronously(times, (_, image, _, _, error) =>
            {
                if (error is not null || image is null)
                    tcs.TrySetException(new FormatException($"Failed to generate thumbnail: {error?.LocalizedDescription}"));
                else
                    tcs.TrySetResult(image);
            });

            using var imageRef = await tcs.Task.ConfigureAwait(false);
            using var image = UIImage.FromImage(imageRef);
            using var jpegData = image.AsJPEG(Constants.Browser.IMAGE_THUMBNAIL_QUALITY / 100f);
            if (jpegData is null)
                throw new FormatException("Failed to convert image to JPEG.");

            return await ToImageStreamAsync(jpegData).ConfigureAwait(false);
        }

        private static NSData? EncodeImage(UIImage image, CGImageAlphaInfo alphaInfo)
        {
            // Use PNG for images with transparency. AsJPEG() expects the compression quality in the 0..1f range
            var hasAlpha = alphaInfo is not (CGImageAlphaInfo.None or CGImageAlphaInfo.NoneSkipFirst or CGImageAlphaInfo.NoneSkipLast);
            return hasAlpha ? image.AsPNG() : image.AsJPEG(Constants.Browser.IMAGE_THUMBNAIL_QUALITY / 100f);
        }

        private static async Task<IImageStream> ToImageStreamAsync(NSData imageData)
        {
            var memoryStream = new MemoryStream((int)imageData.Length);
            await imageData.AsStream().CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0L;

            return new ImageStreamSource(new NonDisposableStream(memoryStream));
        }
    }
}
