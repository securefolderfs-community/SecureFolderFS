using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
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
            Title = !SettingsService.UserSettings.AreFileExtensionsEnabled
                ? Path.GetFileNameWithoutExtension(file.Name)
                : file.Name;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Thumbnail?.Dispose();

            if (!SettingsService.UserSettings.AreThumbnailsEnabled)
                return;

            var typeHint = FileTypeHelper.GetTypeHint(File);
            if (typeHint is TypeHint.Image or TypeHint.Media)
                Thumbnail = await MediaService.TryGenerateThumbnailAsync(File, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        protected override void UpdateStorable(IStorable storable)
        {
            File = (IFile)storable;
        }

        /// <inheritdoc/>
        protected override async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (ParentFolder is null)
                return;

            if (BrowserViewModel.TransferViewModel?.IsPickingItems() ?? false)
                return;

            using var viewModel = new PreviewerOverlayViewModel(this, ParentFolder);
            await viewModel.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(viewModel);
        }
    }
}