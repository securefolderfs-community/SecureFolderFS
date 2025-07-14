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
    public sealed partial class VideoPreviewerViewModel : FilePreviewerViewModel, IDisposable
    {
        private bool _isLateInitialized;

        [ObservableProperty] private IDisposable? _VideoSource;

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
                VideoSource?.Dispose();

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
            VideoSource?.Dispose();

            var streamedVideo = await MediaService.StreamVideoAsync(Inner, cancellationToken);
            if (streamedVideo is IAsyncInitialize asyncInitialize)
                await asyncInitialize.InitAsync(cancellationToken);

            VideoSource = streamedVideo;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VideoSource?.Dispose();
        }
    }
}