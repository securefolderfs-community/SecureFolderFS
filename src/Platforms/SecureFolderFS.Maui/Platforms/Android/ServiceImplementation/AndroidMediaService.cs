using Android.Media;
using OwlCore.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.Helpers;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI;
using Stream = System.IO.Stream;

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
                    
                    return new ImageStream(imageStream);
                }

                case TypeHint.Media:
                {
                    await using var stream = await file.OpenReadAsync(cancellationToken).ConfigureAwait(false);
                    var imageStream = await GenerateVideoThumbnailAsync(stream, TimeSpan.FromSeconds(0)).ConfigureAwait(false);
                    
                    return new ImageStream(imageStream);
                }
                
                default: throw new InvalidOperationException("The provided file type is invalid.");
            }
        }
        
        private static async Task<Stream> GenerateVideoThumbnailAsync(Stream stream, TimeSpan captureTime)
        {
            using var retriever = new MediaMetadataRetriever();
            await retriever.SetDataSourceAsync(new StreamedMediaSource(stream)).ConfigureAwait(false);

            // Use scaled frame for efficiency
            using var bitmap = retriever.GetScaledFrameAtTime(captureTime.Ticks, Option.ClosestSync, 320, 240);
            if (bitmap is null)
                throw new NotSupportedException("Could not retrieve scaled frame.");

            return await ThumbnailHelpers.CompressBitmapAsync(bitmap).ConfigureAwait(false);
        }
    }
}
