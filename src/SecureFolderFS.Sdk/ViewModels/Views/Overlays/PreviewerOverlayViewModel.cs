using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class PreviewerOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly BrowserItemViewModel _itemViewModel;
        private readonly FolderViewModel _folderViewModel;

        [ObservableProperty] private bool _IsImmersed;
        [ObservableProperty] private BasePreviewerViewModel? _PreviewerViewModel;

        public PreviewerOverlayViewModel(BrowserItemViewModel itemViewModel, FolderViewModel folderViewModel)
        {
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

        /// <inheritdoc/>
        public void Dispose()
        {
            (PreviewerViewModel as IDisposable)?.Dispose();
        }
    }
}
