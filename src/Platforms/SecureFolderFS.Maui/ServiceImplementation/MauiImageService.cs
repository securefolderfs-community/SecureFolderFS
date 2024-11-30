using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.Services;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IImageService"/>
    internal sealed class MauiImageService : IImageService
    {
        /// <inheritdoc/>
        public async Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken)
        {
            var stream = await file.OpenReadAsync(cancellationToken);
            return new ImageStream(stream);
        }
    }
}
