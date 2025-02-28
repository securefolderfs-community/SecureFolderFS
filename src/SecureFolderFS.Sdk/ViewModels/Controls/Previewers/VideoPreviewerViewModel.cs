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
    public sealed partial class VideoPreviewerViewModel : BasePreviewerViewModel<IDisposable>, IDisposable
    {
        private readonly IFile _file;

        public VideoPreviewerViewModel(IFile file)
        {
            _file = file;
            ServiceProvider = DI.Default;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var streamedVideo = await MediaService.StreamVideoAsync(_file, cancellationToken);
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