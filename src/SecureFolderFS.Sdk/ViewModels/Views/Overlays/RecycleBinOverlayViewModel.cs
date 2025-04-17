using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IRecycleBinService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        private IRecycleBinFolder? _recycleBin;

        [ObservableProperty] private bool _IsSelecting;
        [ObservableProperty] private bool _IsRecycleBinEnabled;
        [ObservableProperty] private ObservableCollection<RecycleBinItemViewModel> _Items;
        [ObservableProperty] private UnlockedVaultViewModel _UnlockedVaultViewModel;

        public INavigator OuterNavigator { get; }

        public RecycleBinOverlayViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator outerNavigator)
        {
            ServiceProvider = DI.Default;
            Items = new();
            Title = "RecycleBin".ToLocalized();
            UnlockedVaultViewModel = unlockedVaultViewModel;
            OuterNavigator = outerNavigator;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            _recycleBin ??= await RecycleBinService.TryGetRecycleBinAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);
            if (_recycleBin is null)
                return;

            IsRecycleBinEnabled = UnlockedVaultViewModel.StorageRoot.Options.IsRecycleBinEnabled();
            await foreach (var item in _recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item is not IRecycleBinItem recycleBinItem)
                    continue;

                Items.Add(new(this, recycleBinItem.Inner, _recycleBin)
                {
                    Title = recycleBinItem.Name,
                    DeletionTimestamp = recycleBinItem.DeletionTimestamp
                });
            }
        }

        [RelayCommand]
        private async Task ToggleRecycleBinAsync(CancellationToken cancellationToken)
        {
            var isEnabled = IsRecycleBinEnabled;
            await RecycleBinService.ConfigureRecycleBinAsync(
                UnlockedVaultViewModel.StorageRoot,
                isEnabled ? 0L : -1L,
                cancellationToken);

            IsRecycleBinEnabled = !isEnabled;
            if (IsRecycleBinEnabled)
                _recycleBin ??= await RecycleBinService.TryGetOrCreateRecycleBinAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);
        }

        [RelayCommand]
        public void ToggleSelection(bool? value = null)
        {
            IsSelecting = value ?? !IsSelecting;
            Items.UnselectAll();
        }

        [RelayCommand]
        public void SelectAll()
        {
            IsSelecting = true;
            Items.SelectAll();
        }
    }
}