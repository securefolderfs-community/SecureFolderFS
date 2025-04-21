using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage
{
    [Bindable(true)]
    [Inject<IRecycleBinService>, Inject<IApplicationService>]
    public sealed partial class RecycleBinItemViewModel : StorageItemViewModel
    {
        private readonly IRecycleBinFolder _recycleBin;

        [ObservableProperty] private string? _OriginalPath;
        [ObservableProperty] private DateTime? _DeletionTimestamp;
        [ObservableProperty] private RecycleBinOverlayViewModel _OverlayViewModel;

        /// <inheritdoc/>
        public override IStorable Inner { get; }

        public RecycleBinItemViewModel(RecycleBinOverlayViewModel overlayViewModel, IRecycleBinItem recycleBinItem, IRecycleBinFolder recycleBin)
        {
            ServiceProvider = DI.Default;
            OverlayViewModel = overlayViewModel;
            Inner = recycleBinItem.Inner;
            Title = recycleBinItem.Name;
            OriginalPath = recycleBinItem.Id;
            DeletionTimestamp = recycleBinItem.DeletionTimestamp;
            _recycleBin = recycleBin;
        }

        [RelayCommand]
        private async Task RestoreAsync(CancellationToken cancellationToken)
        {
            var items = OverlayViewModel.IsSelecting ? OverlayViewModel.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            IFolderPicker folderPicker = ApplicationService.IsDesktop
                ? DI.Service<IFileExplorerService>()
                : BrowserHelpers.CreateBrowser(OverlayViewModel.UnlockedVaultViewModel, outerNavigator: OverlayViewModel.OuterNavigator);

            if (await _recycleBin.TryRestoreItemsAsync(items.Select(x => x.Inner as IStorableChild)!, folderPicker, cancellationToken))
            {
                foreach (var item in items)
                    OverlayViewModel.Items.Remove(item);
            }
            OverlayViewModel.ToggleSelection(false);
        }

        [RelayCommand]
        private async Task DeletePermanentlyAsync(CancellationToken cancellationToken)
        {
            var items = OverlayViewModel.IsSelecting ? OverlayViewModel.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            foreach (var item in items)
            {
                if (item.Inner is not IStorableChild innerChild)
                    continue;

                try
                {
                    await _recycleBin.DeleteAsync(innerChild, cancellationToken);
                    OverlayViewModel.Items.Remove(item);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            OverlayViewModel.ToggleSelection(false);
        }
    }
}
