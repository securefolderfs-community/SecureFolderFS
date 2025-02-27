using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
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
        public async Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken)
        {
            var classification = FileTypeHelper.GetClassification(file);
            var stream = await file.OpenReadAsync(cancellationToken);

            return new VideoStreamServer(stream, classification.MimeType);
        }
    }
}
