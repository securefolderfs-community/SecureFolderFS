using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Bindable(true)]
    [Inject<IRecycleBinService>, Inject<IApplicationService>]
    public sealed partial class RecycleBinItemViewModel : StorageItemViewModel
    {
        [ObservableProperty] private DateTime? _DeletionTimestamp;
        [ObservableProperty] private RecycleBinOverlayViewModel _OverlayViewModel;

        /// <inheritdoc/>
        public override IStorable Inner { get; }

        public RecycleBinItemViewModel(IStorableChild ciphertextItem, RecycleBinOverlayViewModel overlayViewModel)
        {
            ServiceProvider = DI.Default;
            Inner = ciphertextItem;
            OverlayViewModel = overlayViewModel;
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
            
            foreach (var item in items)
            {
                if (item.Inner is not IStorableChild innerChild)
                    continue;
                
                var result = await RecycleBinService.RestoreItemAsync(OverlayViewModel.UnlockedVaultViewModel.StorageRoot, innerChild, folderPicker, cancellationToken);
                if (result.Successful)
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
                
                var result = await RecycleBinService.DeletePermanentlyAsync(innerChild, cancellationToken);
                if (result.Successful)
                    OverlayViewModel.Items.Remove(item);
            }
            
            OverlayViewModel.ToggleSelection(false);
        }
    }
}
