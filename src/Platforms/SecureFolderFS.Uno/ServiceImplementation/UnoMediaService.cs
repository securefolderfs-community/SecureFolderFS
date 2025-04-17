using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Uno.AppModels;
using SecureFolderFS.Uno.Helpers;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class UnoMediaService : IMediaService
    {
        /// <inheritdoc/>
        public async Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken)
        {
            await using var stream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);
            using var winrtStream = stream.AsRandomAccessStream();

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(winrtStream).AsTask(cancellationToken);

            return new ImageBitmap(bitmapImage, null);

            // TODO: Check if it works
            var classification = FileTypeHelper.GetClassification(file);
            return await ImagingHelpers.GetBitmapFromStreamAsync(winrtStream, classification.MimeType, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IImageStream?> GenerateThumbnailAsync(IFile file, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IImageStream?>(null);
        }

        /// <inheritdoc/>
        public Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken)
        {
            return Task.FromException<IDisposable>(new NotSupportedException());
        }
    }
}
