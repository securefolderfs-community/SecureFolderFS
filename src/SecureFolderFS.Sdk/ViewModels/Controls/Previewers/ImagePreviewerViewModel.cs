using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Inject<IMediaService>]
    [Bindable(true)]
    public sealed partial class ImagePreviewerViewModel : FilePreviewerViewModel<IImage>, IDisposable
    {
        public ImagePreviewerViewModel(IFile file)
            : base(file)
        {
            ServiceProvider = DI.Default;
            // We do not want to set the Title property for an image
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Source = await MediaService.ReadImageFileAsync(Inner, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Source?.Dispose();
        }
    }
}
