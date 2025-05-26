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
        private bool _isLateInitialized;

        public VideoPreviewerViewModel(IFile file, bool isLateInitialized)
            : base(file)
        {
            ServiceProvider = DI.Default;
            Title = file.Name;
            _isLateInitialized = isLateInitialized;
        }

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            if (_isLateInitialized)
                await CreateSourceAsync(CancellationToken.None);

            base.OnAppearing();
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            if (_isLateInitialized)
                Source?.Dispose();

            base.OnDisappearing();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (!_isLateInitialized)
                await CreateSourceAsync(cancellationToken);
        }

        private async Task CreateSourceAsync(CancellationToken cancellationToken)
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