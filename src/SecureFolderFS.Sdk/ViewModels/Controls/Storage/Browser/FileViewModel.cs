using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<IOverlayService>, Inject<IMediaService>, Inject<ISettingsService>]
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
            Thumbnail?.Dispose();
            
            if (SettingsService.UserSettings.AreThumbnailsEnabled)
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
            if (BrowserViewModel.TransferViewModel?.IsPickingItems() ?? false)
                return;
            
            using var viewModel = new PreviewerOverlayViewModel();
            await viewModel.LoadFromStorableAsync(Inner, cancellationToken);
            await OverlayService.ShowAsync(viewModel);
        }
    }
}