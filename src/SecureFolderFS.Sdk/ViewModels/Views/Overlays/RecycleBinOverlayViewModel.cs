using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IRecycleBinService>, Inject<ISystemService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        private IRecycleBinFolder? _recycleBin;

        [ObservableProperty] private bool _IsSelecting;
        [ObservableProperty] private bool _IsRecycleBinEnabled;
        [ObservableProperty] private PickerOptionViewModel? _CurrentSizeOption;
        [ObservableProperty] private UnlockedVaultViewModel _UnlockedVaultViewModel;
        [ObservableProperty] private ObservableCollection<RecycleBinItemViewModel> _Items;
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _SizeOptions;

        public INavigator OuterNavigator { get; }

        public RecycleBinOverlayViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator outerNavigator)
        {
            ServiceProvider = DI.Default;
            Items = new();
            Title = "RecycleBin".ToLocalized();
            SizeOptions = new();
            OuterNavigator = outerNavigator;
            UnlockedVaultViewModel = unlockedVaultViewModel;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get storage root folder
            var rootFolder = UnlockedVaultViewModel.VaultViewModel.VaultModel.Folder;
            if (rootFolder is IChildFolder childFolder)
                rootFolder = await childFolder.GetRootAsync(cancellationToken) ?? childFolder;

            // Get and populate available size options
            var deviceFreeSpace = await SystemService.GetAvailableFreeSpaceAsync(rootFolder, cancellationToken);
            var sizeOptions = RecycleBinHelpers.GetSizeOptions(deviceFreeSpace);
            SizeOptions.AddMultiple(sizeOptions);

            // Choose the saved size option
            CurrentSizeOption = SizeOptions.FirstOrDefault(x => long.Parse(x.Id) == UnlockedVaultViewModel.StorageRoot.Options.RecycleBinSize)
                ?? SizeOptions.ElementAtOrDefault(1)
                ?? SizeOptions.FirstOrDefault();

            // TODO: Is the following logic correct?
            // Try and retrieve recycle bin for later use
            _recycleBin ??= await RecycleBinService.TryGetRecycleBinAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);
            if (_recycleBin is null)
                return;

            // We can only determine that the recycle bin is enabled if it exists
            IsRecycleBinEnabled = UnlockedVaultViewModel.StorageRoot.Options.IsRecycleBinEnabled();
            await foreach (var item in _recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item is not IRecycleBinItem recycleBinItem)
                    continue;

                Items.Add(new(this, recycleBinItem, _recycleBin));
            }
        }

        [RelayCommand]
        private async Task UpdateRecycleBinAsync(bool? value, CancellationToken cancellationToken)
        {
            if (value is not { } bValue)
                return;

            if (!long.TryParse(CurrentSizeOption?.Id, out var size))
                return;

            await RecycleBinService.ConfigureRecycleBinAsync(
                UnlockedVaultViewModel.StorageRoot,
                bValue ? size : 0L,
                cancellationToken);

            IsRecycleBinEnabled = bValue;
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