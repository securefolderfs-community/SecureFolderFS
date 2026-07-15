using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IOverlayService>, Inject<IShareService>]
    public sealed partial class PreviewerOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        // Text files are loaded into memory in full; refuse to preview unreasonably large ones
        private const long MAX_TEXT_PREVIEW_SIZE = 10 * 1024 * 1024;

        private readonly BrowserItemViewModel _itemViewModel;
        private readonly FolderViewModel _folderViewModel;

        [ObservableProperty] private bool _IsImmersed;
        [ObservableProperty] private BasePreviewerViewModel? _PreviewerViewModel;

        /// <summary>
        /// Occurs when the previewer requests to be closed, e.g. after the previewed item was deleted.
        /// </summary>
        public event EventHandler? CloseRequested;

        /// <summary>
        /// Gets the command that deletes the currently previewed item, or null when the vault is read-only.
        /// </summary>
        public IAsyncRelayCommand? DeleteItemCommand => _folderViewModel.BrowserViewModel.Options.IsReadOnly ? null : DeleteCurrentCommand;

        public PreviewerOverlayViewModel(BrowserItemViewModel itemViewModel, FolderViewModel folderViewModel)
        {
            ServiceProvider = DI.Default;
            _itemViewModel = itemViewModel;
            _folderViewModel = folderViewModel;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (_itemViewModel.Inner is not IFile file)
                return;

            var classification = FileTypeHelper.GetClassification(_itemViewModel.Inner);
            var previewer = (BasePreviewerViewModel)(classification.TypeHint switch
            {
                TypeHint.Plaintext when await IsWithinTextSizeLimitAsync(file, cancellationToken)
                    => new TextPreviewerViewModel(file, _folderViewModel.BrowserViewModel.Options.IsReadOnly).WithInitAsync(cancellationToken),
                TypeHint.Document when classification is { MimeType: "application/pdf" } => new PdfPreviewerViewModel(file).WithInitAsync(cancellationToken),
                TypeHint.Image or TypeHint.Media or TypeHint.Audio => new CarouselPreviewerViewModel(
                    _folderViewModel.Items
                        .Where(x => x is FileViewModel && FileTypeHelper.GetTypeHint(x.Inner) is TypeHint.Image or TypeHint.Media or TypeHint.Audio)
                        .Select(x => (FileViewModel)x),
                    (FileViewModel)_itemViewModel).WithInitAsync(cancellationToken),
                TypeHint.Archive => new ArchivePreviewerViewModel(file, _folderViewModel, _folderViewModel.BrowserViewModel.TransferViewModel).WithInitAsync(cancellationToken),
                _ => new FallbackPreviewerViewModel(file).WithInitAsync(cancellationToken)
            });

            (PreviewerViewModel as IDisposable)?.Dispose();
            PreviewerViewModel = previewer;
        }

        private static async Task<bool> IsWithinTextSizeLimitAsync(IFile file, CancellationToken cancellationToken)
        {
            try
            {
                var size = await file.GetSizeAsync(cancellationToken);
                return size is null or <= MAX_TEXT_PREVIEW_SIZE;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                // If the size cannot be determined, attempt the preview anyway
                return true;
            }
        }

        [RelayCommand]
        private void ToggleImmersion()
        {
            IsImmersed = !IsImmersed;
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync(CancellationToken cancellationToken)
        {
            BasePreviewerViewModel? previewer = PreviewerViewModel as FilePreviewerViewModel;
            if (PreviewerViewModel is CarouselPreviewerViewModel carouselPreviewer)
                previewer = carouselPreviewer.Slides[carouselPreviewer.CurrentIndex];

            if (previewer is not FilePreviewerViewModel filePreviewer)
                return;

            var propertiesOverlay = new PropertiesOverlayViewModel(filePreviewer.Inner);
            _ = propertiesOverlay.InitAsync(cancellationToken);

            await OverlayService.ShowAsync(propertiesOverlay);
        }

        [RelayCommand]
        private async Task ShareAsync()
        {
            BasePreviewerViewModel? previewer = PreviewerViewModel as FilePreviewerViewModel;
            if (PreviewerViewModel is CarouselPreviewerViewModel carouselPreviewer)
                previewer = carouselPreviewer.Slides[carouselPreviewer.CurrentIndex];

            if (previewer is not FilePreviewerViewModel filePreviewer)
                return;

            await ShareService.ShareFileAsync(filePreviewer.Inner);
        }

        [RelayCommand]
        private async Task DeleteCurrentAsync(CancellationToken cancellationToken)
        {
            BasePreviewerViewModel? previewer = PreviewerViewModel as FilePreviewerViewModel;
            if (PreviewerViewModel is CarouselPreviewerViewModel carouselPreviewer)
                previewer = carouselPreviewer.Slides.ElementAtOrDefault(carouselPreviewer.CurrentIndex);

            if (previewer is not FilePreviewerViewModel filePreviewer)
                return;

            // Delegate to the browser item so the recycle bin and confirmation flows apply
            var itemViewModel = _folderViewModel.Items.FirstOrDefault(x => x.Inner.Id == filePreviewer.Inner.Id);
            if (itemViewModel is null)
                return;

            await itemViewModel.DeleteCommand.ExecuteAsync(null);

            // The deletion may have been declined in the confirmation prompt or may have failed
            if (_folderViewModel.Items.Any(x => x.Inner.Id == filePreviewer.Inner.Id))
                return;

            if (PreviewerViewModel is CarouselPreviewerViewModel carouselViewModel)
            {
                carouselViewModel.RemoveSlide(filePreviewer);
                if (carouselViewModel.Slides.Count > 0)
                    return;
            }

            // Nothing left to preview
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            (PreviewerViewModel as IDisposable)?.Dispose();
        }
    }
}
