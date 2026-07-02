using OwlCore.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.Helpers;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class AndroidMediaService : BaseMauiMediaService
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
                    var imageStream = await ThumbnailHelpers.GenerateImageThumbnailAsync(stream, Constants.Browser.IMAGE_THUMBNAIL_MAX_SIZE).ConfigureAwait(false);

                    return new ImageStreamSource(imageStream);
                }

                case TypeHint.Media:
                {
                    // Capture at one second rather than the very first frame, which is often
                    // black (fade-ins). ClosestSync clamps the position for shorter videos
                    await using var stream = await file.OpenReadAsync(cancellationToken).ConfigureAwait(false);
                    var imageStream = await ThumbnailHelpers.GenerateVideoThumbnailAsync(stream, TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                    return new ImageStreamSource(imageStream);
                }

                default: throw new InvalidOperationException("The provided file type is invalid.");
            }
        }
    }
}
