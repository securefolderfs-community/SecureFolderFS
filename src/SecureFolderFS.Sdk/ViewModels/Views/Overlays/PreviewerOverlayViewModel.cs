using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class PreviewerOverlayViewModel : OverlayViewModel, IDisposable
    {
        [ObservableProperty] private IViewable? _PreviewerViewModel;

        public async Task LoadFromStorableAsync(IStorable storable, CancellationToken cancellationToken = default)
        {
            // Only handle files for now
            if (storable is not IFile file)
                return;

            var classification = FileTypeHelper.GetClassification(storable);
            var previewer = (IAsyncInitialize?)(classification.TypeHint switch
            {
                TypeHint.Image => new ImagePreviewerViewModel(file),
                TypeHint.Media => new VideoPreviewerViewModel(file),
                TypeHint.PlainText => new TextPreviewerViewModel(file),
                _ => new FallbackPreviewerViewModel(file)
            });

            if (previewer is null)
                return;

            await previewer.InitAsync(cancellationToken);
            (PreviewerViewModel as IDisposable)?.Dispose();
            PreviewerViewModel = previewer as IViewable;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            (PreviewerViewModel as IDisposable)?.Dispose();
        }
    }
}
