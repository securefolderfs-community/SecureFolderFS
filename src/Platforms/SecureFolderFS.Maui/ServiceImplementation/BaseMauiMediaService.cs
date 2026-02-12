using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Enums;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;
using Stream = System.IO.Stream;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal abstract class BaseMauiMediaService : IMediaService
    {
        /// <inheritdoc/>
        public virtual async Task<IImage> GetImageFromResourceAsync(string resourceName, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return resourceName switch
            {
                "Windows_Device" => new ImageResourceFile(MauiThemeHelper.Instance.ActualTheme switch
                {
                    ThemeType.Light => "surface3_light.png",
                    ThemeType.Dark => "surface3_dark.png"
                }),

                "MacOS_Device" => new ImageResourceFile(MauiThemeHelper.Instance.ActualTheme switch
                {
                    ThemeType.Light => "mbpro_light.png",
                    ThemeType.Dark => "mbpro_dark.png"
                }),

                "Linux_Device" or "Unknown_Device" => new ImageResourceFile("generic_laptop.png"),

                _ => new ImageResourceFile(resourceName, true)
            };
        }

        /// <inheritdoc/>
        public virtual Task<IImage> GetImageFromUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            var remoteImageUrl = new ImageRemoteUrl(url);
            return Task.FromResult<IImage>(remoteImageUrl);
        }

        /// <inheritdoc/>
        public virtual async Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken)
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
        public virtual async Task<IDisposable> StreamPdfSourceAsync(IFile file, CancellationToken cancellationToken = default)
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
    }
}
