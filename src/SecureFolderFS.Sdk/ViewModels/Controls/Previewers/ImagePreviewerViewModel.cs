using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Inject<IMediaService>]
    [Bindable(true)]
    public sealed partial class ImagePreviewerViewModel : FilePreviewerViewModel, IDisposable
    {
        [ObservableProperty] private IImage? _Image;

        public ImagePreviewerViewModel(IFile file)
            : base(file)
        {
            ServiceProvider = DI.Default;
            Title = file.Name;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Image?.Dispose();
            Image = await MediaService.ReadImageFileAsync(Inner, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
