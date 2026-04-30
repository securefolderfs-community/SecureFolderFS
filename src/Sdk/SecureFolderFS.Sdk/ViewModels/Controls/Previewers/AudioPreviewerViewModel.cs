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
    public sealed partial class AudioPreviewerViewModel : FilePreviewerViewModel, IDisposable
    {
        private bool _isLateInitialized;

        [ObservableProperty] private IDisposable? _AudioSource;

        public AudioPreviewerViewModel(IFile file, bool isLateInitialized)
            : base(file)
        {
            ServiceProvider = DI.Default;
            Title = file.Name;
            IsToolbarOnTop = true;
            _isLateInitialized = isLateInitialized;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (!_isLateInitialized)
                await CreateSourceAsync(cancellationToken);
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
                AudioSource?.Dispose();

            base.OnDisappearing();
        }

        private async Task CreateSourceAsync(CancellationToken cancellationToken)
        {
            AudioSource?.Dispose();

            var streamedAudio = await MediaService.StreamAudioAsync(Inner, cancellationToken);
            if (streamedAudio is IAsyncInitialize asyncInitialize)
                await asyncInitialize.InitAsync(cancellationToken);

            AudioSource = streamedAudio;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            AudioSource?.Dispose();
        }
    }
}