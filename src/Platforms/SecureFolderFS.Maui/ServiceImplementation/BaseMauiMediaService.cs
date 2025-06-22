using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;
using Stream = System.IO.Stream;

#if IOS || MACCATALYST
using AVFoundation;
using CoreMedia;
using Foundation;
using UIKit;
#endif

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal abstract class BaseMauiMediaService : IMediaService
    {
        /// <inheritdoc/>
        public async Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken)
        {
            var stream = await file.OpenReadAsync(cancellationToken);
            return new ImageStream(stream);
        }

        /// <inheritdoc/>
        public virtual async Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken)
        {
            var stream = await file.OpenReadAsync(cancellationToken);
            return new AggregatedDisposable([stream]);
        }
        
        /// <inheritdoc/>
        public async Task<IDisposable> StreamPdfSourceAsync(IFile file, CancellationToken cancellationToken = default)
        {
            var classification = FileTypeHelper.GetClassification(file);
            var stream = await file.OpenReadAsync(cancellationToken);
            
            return new PdfStreamServer(stream, classification.MimeType);
        }

        /// <inheritdoc/>
        public virtual Task<bool> TrySetFolderIconAsync(IModifiableFolder folder, Stream imageStream, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public abstract Task<IImageStream> GenerateThumbnailAsync(IFile file, TypeHint typeHint = default, CancellationToken cancellationToken = default);

        // TODO: Move to iOS folder
#if IOS || MACCATALYST
        private static void IOS_ExtractFrame(string videoPath, string outputPath, TimeSpan captureTime)
        {
            // var asset = new AVAsset();
            // var generator = new AVAssetImageGenerator(asset) { AppliesPreferredTrackTransform = true };
            // var time = CMTime.FromSeconds(captureTime.TotalSeconds, 1);
            //
            // NSError error;
            // var imageRef = generator.CopyCGImageAtTime(time, out error);
            // if (imageRef != null)
            // {
            //     using var image = new UIImage(imageRef);
            //     using var data = image.AsJPEG(0.8f);
            //     File.WriteAllBytes(outputPath, data.ToArray());
            // }
        }
#endif
    }
}
