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
    public sealed partial class ImagePreviewerViewModel : BasePreviewerViewModel<IImage>, IDisposable
    {
        private readonly IFile? _file;
        
        public ImagePreviewerViewModel(IFile file)
        {
            ServiceProvider = DI.Default;
            _file = file;
            // We do not want to set the Title property for an image
        }

        public ImagePreviewerViewModel(IImage image)
        {
            ServiceProvider = DI.Default;
            Source = image;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (_file is not null)
                Source = await MediaService.ReadImageFileAsync(_file, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Source?.Dispose();
        }
    }
}
