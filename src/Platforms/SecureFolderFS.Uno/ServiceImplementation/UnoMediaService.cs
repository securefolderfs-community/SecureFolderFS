using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Uno.AppModels;

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

            //var mimeType = MimeTypeMap.GetMimeType(file.Id);
            //return await ImagingHelpers.GetBitmapFromStreamAsync(winrtStream, mimeType, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken)
        {
            return Task.FromException<IDisposable>(new NotSupportedException());
        }
    }
}
