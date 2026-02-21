using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;

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

        /// <summary>
        /// Gets the classification of the file.
        /// </summary>
        public TypeClassification Classification { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether a thumbnail can be loaded for the associated file.
        /// </summary>
        public virtual bool CanLoadThumbnail => Thumbnail is null && Classification.TypeHint is TypeHint.Image or TypeHint.Media;

        public FileViewModel(IFile file, BrowserViewModel browserViewModel, FolderViewModel? parentFolder)
            : base(browserViewModel, parentFolder)
        {
            ServiceProvider = DI.Default;
            File = file;
            Title = !SettingsService.UserSettings.AreFileExtensionsEnabled
                ? Path.GetFileNameWithoutExtension(file.Name)
                : file.Name;
            Classification = FileTypeHelper.GetClassification(file);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Thumbnail?.Dispose();

            if (!SettingsService.UserSettings.AreThumbnailsEnabled)
                return;

            if (!CanLoadThumbnail)
                return;

            // Try to get from the cache first
            var cachedStream = await BrowserViewModel.ThumbnailCache.TryGetCachedThumbnailAsync(File, cancellationToken);
            if (cachedStream is not null)
            {
                Thumbnail = new StreamImageModel(cachedStream);
                return;
            }

            // Generate a new thumbnail
            var generatedThumbnail = await MediaService.TryGenerateThumbnailAsync(File, Classification.TypeHint, cancellationToken);
            if (generatedThumbnail is null)
                return;

            // Cache the generated thumbnail
            await BrowserViewModel.ThumbnailCache.CacheThumbnailAsync(File, generatedThumbnail, cancellationToken);

            Thumbnail = generatedThumbnail;
        }

        /// <inheritdoc/>
        protected override void UpdateStorable(IStorable storable)
        {
            File = (IFile)storable;
            Classification = FileTypeHelper.GetClassification(storable);
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

            if (BrowserViewModel.Options.IsReadOnly)
                return;

            if (viewModel.PreviewerViewModel is IChangeTracker { WasModified: true } and IPersistable persistable)
            {
                var messageOverlay = new MessageOverlayViewModel()
                {
                    Title = "UnsavedChanges".ToLocalized(),
                    Message = "UnsavedChangesDescription".ToLocalized(),
                    PrimaryText = "Save".ToLocalized(),
                    SecondaryText = "Cancel".ToLocalized()
                };

                await Task.Delay(700);
                var result = await OverlayService.ShowAsync(messageOverlay);
                if (!result.Positive())
                    return;

                if (BrowserViewModel.TransferViewModel is not { } transferViewModel)
                {
                    await persistable.SaveAsync(cancellationToken);
                    return;
                }

                transferViewModel.TransferType = TransferType.Save;
                using var saveCancellation = transferViewModel.GetCancellation();
                await transferViewModel.PerformOperationAsync(async ct =>
                {
                    await persistable.SaveAsync(ct);
                }, saveCancellation.Token);
            }
        }
    }
}