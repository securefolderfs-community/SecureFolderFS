using System;
using System.ComponentModel;
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
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IOverlayService>]
    public sealed partial class PreviewerOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly BrowserItemViewModel _itemViewModel;
        private readonly FolderViewModel _folderViewModel;

        [ObservableProperty] private bool _IsImmersed;
        [ObservableProperty] private BasePreviewerViewModel? _PreviewerViewModel;

        public PreviewerOverlayViewModel(BrowserItemViewModel itemViewModel, FolderViewModel folderViewModel)
        {
            ServiceProvider = DI.Default;
            _itemViewModel = itemViewModel;
            _folderViewModel = folderViewModel;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (_itemViewModel.Inner is not IFile file)
                return Task.CompletedTask;

            var classification = FileTypeHelper.GetClassification(_itemViewModel.Inner);
            var previewer = (BasePreviewerViewModel)(classification.TypeHint switch
            {
                TypeHint.Image or TypeHint.Media => new CarouselPreviewerViewModel(_folderViewModel, _itemViewModel).WithInitAsync(),
                TypeHint.Plaintext => new TextPreviewerViewModel(file, _folderViewModel.BrowserViewModel.Options.IsReadOnly).WithInitAsync(),
                TypeHint.Document when classification is { MimeType: "application/pdf" } => new PdfPreviewerViewModel(file).WithInitAsync(),
                _ => new FallbackPreviewerViewModel(file).WithInitAsync()
            });

            (PreviewerViewModel as IDisposable)?.Dispose();
            PreviewerViewModel = previewer;

            return Task.CompletedTask;
        }

        [RelayCommand]
        private void ToggleImmersion()
        {
            IsImmersed = !IsImmersed;
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync(CancellationToken cancellationToken)
        {
            if (PreviewerViewModel is not FilePreviewerViewModel { Inner:  IStorableProperties storableProperties } filePreviewer)
                return;

            var properties = await storableProperties.GetPropertiesAsync();
            var propertiesOverlay = new PropertiesOverlayViewModel(filePreviewer.Inner, properties);
            _ = propertiesOverlay.InitAsync(cancellationToken);

            await OverlayService.ShowAsync(propertiesOverlay);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            (PreviewerViewModel as IDisposable)?.Dispose();
        }
    }
}
