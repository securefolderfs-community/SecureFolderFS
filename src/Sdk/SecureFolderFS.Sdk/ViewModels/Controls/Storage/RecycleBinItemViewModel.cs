using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage
{
    [Bindable(true)]
    [Inject<IRecycleBinService>, Inject<IApplicationService>, Inject<IMediaService>]
    public sealed partial class RecycleBinItemViewModel : StorageItemViewModel, IAsyncInitialize
    {
        private readonly IRecycleBinFolder _recycleBin;
        private readonly IRecycleBinItem _recycleBinItem;

        [ObservableProperty] private string? _Size;
        [ObservableProperty] private string? _OriginalPath;
        [ObservableProperty] private DateTime? _DeletionTimestamp;
        [ObservableProperty] private RecycleBinOverlayViewModel _OverlayViewModel;

        /// <inheritdoc/>
        public override IStorable Inner { get; }

        public RecycleBinItemViewModel(RecycleBinOverlayViewModel overlayViewModel, IRecycleBinItem recycleBinItem, IRecycleBinFolder recycleBin)
        {
            ServiceProvider = DI.Default;
            _recycleBinItem = recycleBinItem;
            OverlayViewModel = overlayViewModel;
            Inner = recycleBinItem.Inner;
            Title = recycleBinItem.Name;
            OriginalPath = recycleBinItem.Id;
            _recycleBin = recycleBin;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is not IFile file)
                return;

            var size = await _recycleBinItem.SizeOf.GetValueAsync(cancellationToken);
            Size = size.HasValue ? ByteSize.FromBytes(size.Value).ToString().Replace(" ", string.Empty) : string.Empty;
            DeletionTimestamp = await _recycleBinItem.CreatedAt.GetValueAsync(cancellationToken);

            var extension = Path.GetExtension(Title);
            if (extension is null)
                return;

            var typeHint = FileTypeHelper.GetTypeHintFromExtension(extension);
            if (typeHint is TypeHint.Image or TypeHint.Media)
                Thumbnail = await MediaService.TryGenerateThumbnailAsync(file, typeHint, cancellationToken);
        }

        [RelayCommand]
        private async Task RestoreAsync(CancellationToken cancellationToken)
        {
            RecycleBinItemViewModel[] items;
            if (ApplicationService.IsDesktop)
            {
                items = [this];
            }
            else
            {
                items = OverlayViewModel.IsSelecting ? OverlayViewModel.Items.GetSelectedItems().ToArray() : [];
                if (items.IsEmpty())
                    items = [this];
            }

            IFolderPicker folderPicker = ApplicationService.IsDesktop
                ? DI.Service<IFileExplorerService>()
                : BrowserHelpers.CreateBrowser(
                    OverlayViewModel.UnlockedVaultViewModel.StorageRoot.PlaintextRoot,
                    OverlayViewModel.UnlockedVaultViewModel.Options,
                    OverlayViewModel.UnlockedVaultViewModel,
                    outerNavigator: OverlayViewModel.OuterNavigator);

            var itemsToRestore = items.Select(x => x.AsWrapper<IStorable>().GetWrapperAt(1).Inner).Cast<IStorableChild>().ToArray();
            if (await _recycleBin.TryRestoreItemsAsync(itemsToRestore, folderPicker, cancellationToken))
            {
                foreach (var item in items)
                    OverlayViewModel.Items.Remove(item);
            }

            OverlayViewModel.ToggleSelectionCommand.Execute(false);
        }

        [RelayCommand]
        private async Task DeletePermanentlyAsync(CancellationToken cancellationToken)
        {
            RecycleBinItemViewModel[] items;
            if (ApplicationService.IsDesktop)
            {
                items = [this];
            }
            else
            {
                items = OverlayViewModel.IsSelecting ? OverlayViewModel.Items.GetSelectedItems().ToArray() : [];
                if (items.IsEmpty())
                    items = [this];
            }

            foreach (var item in items)
            {
                if (item.AsWrapper<IStorable>().GetWrapperAt(1).Inner is not IStorableChild innerChild)
                    continue;

                try
                {
                    await _recycleBin.DeleteAsync(innerChild, cancellationToken);
                    OverlayViewModel.Items.Remove(item);
                }
                catch (Exception ex)
                {
                    _ = ex;
                }
            }

            OverlayViewModel.ToggleSelectionCommand.Execute(false);
        }
    }
}
