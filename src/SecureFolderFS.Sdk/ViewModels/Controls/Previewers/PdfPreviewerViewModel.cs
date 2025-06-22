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
    [Bindable(true)]
    [Inject<IMediaService>]
    public sealed partial class PdfPreviewerViewModel : FilePreviewerViewModel, IDisposable
    {
        [ObservableProperty] private IDisposable? _PdfSource;

        public PdfPreviewerViewModel(IFile file)
            : base(file)
        {
            ServiceProvider = DI.Default;
            Title = file.Name;
            IsToolbarOnTop = true;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            PdfSource?.Dispose();
            var pdfSource = await MediaService.StreamPdfSourceAsync(Inner, cancellationToken);
            if (pdfSource is IAsyncInitialize asyncInitialize)
                await asyncInitialize.InitAsync(cancellationToken);

            PdfSource = pdfSource;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            PdfSource?.Dispose();
        }
    }
}