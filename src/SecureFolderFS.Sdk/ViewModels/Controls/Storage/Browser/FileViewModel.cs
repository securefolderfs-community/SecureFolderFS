using System.Collections.Generic;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<IOverlayService>, Inject<IMediaService>]
    [Bindable(true)]
    public partial class FileViewModel : BrowserItemViewModel
    {
        /// <inheritdoc/>
        public override IStorable Inner => File;

        /// <summary>
        /// Gets the file associated with this view model.
        /// </summary>
        public IFile File { get; protected set; }

        public FileViewModel(IFile file, BrowserViewModel browserViewModel, FolderViewModel? parentFolder)
            : base(browserViewModel, parentFolder)
        {
            ServiceProvider = DI.Default;
            File = file;
            Title = file.Name;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Thumbnail = await MediaService.GenerateThumbnailAsync(File, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void UpdateStorable(IStorable storable)
        {
            File = (IFile)storable;
        }

        /// <inheritdoc/>
        protected override async Task OpenAsync(CancellationToken cancellationToken)
        {
            using var viewModel = new PreviewerOverlayViewModel();
            await viewModel.LoadFromStorableAsync(Inner, cancellationToken);
            await OverlayService.ShowAsync(viewModel);
        }
    }
}