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
    public sealed partial class VideoPreviewerViewModel : FilePreviewerViewModel<IDisposable>, IDisposable
    {
        public VideoPreviewerViewModel(IFile file)
            : base(file)
        {
            ServiceProvider = DI.Default;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Source?.Dispose();

            var streamedVideo = await MediaService.StreamVideoAsync(Inner, cancellationToken);
            if (streamedVideo is IAsyncInitialize asyncInitialize)
                await asyncInitialize.InitAsync(cancellationToken);

            Source = streamedVideo;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Source?.Dispose();
        }
    }
}