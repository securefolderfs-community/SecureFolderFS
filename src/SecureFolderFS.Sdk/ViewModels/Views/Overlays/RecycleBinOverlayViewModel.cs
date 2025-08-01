using System.Collections.ObjectModel;
using System.ComponentModel;
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
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IRecycleBinService>, Inject<ISystemService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        private long _occupiedSize;
        private IRecycleBinFolder? _recycleBin;

        [ObservableProperty] private bool _IsSelecting;
        [ObservableProperty] private bool _IsRecycleBinEnabled;
        [ObservableProperty] private string? _SpaceTakenText;
        [ObservableProperty] private double _PercentageTaken;
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

            // TODO: Is the following logic (order) correct?
            // Try and retrieve recycle bin for later use
            _recycleBin ??= await RecycleBinService.TryGetRecycleBinAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);
            if (_recycleBin is null)
                return;

            // Get occupied state
            if (CurrentSizeOption is not null && CurrentSizeOption.Id != "-1")
            {
                _occupiedSize = await _recycleBin.GetSizeAsync(cancellationToken);
                UpdateSizeBar(CurrentSizeOption);
            }

            // We can only determine that the recycle bin is enabled if it exists
            IsRecycleBinEnabled = UnlockedVaultViewModel.StorageRoot.Options.IsRecycleBinEnabled();
            await foreach (var item in _recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item is not IRecycleBinItem recycleBinItem)
                    continue;

                Items.Add(new RecycleBinItemViewModel(this, recycleBinItem, _recycleBin).WithInitAsync(cancellationToken));
            }
        }

        /// <summary>
        /// Toggles the recycle bin on or off updating the configuration file.
        /// </summary>
        /// <param name="value">The value that determines whether to enable or disable the recycle bin.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        public async Task ToggleRecycleBinAsync(bool value, CancellationToken cancellationToken = default)
        {
            if (!long.TryParse(CurrentSizeOption?.Id, out var size))
                return;

            var result = await RecycleBinService.TryConfigureRecycleBinAsync(
                UnlockedVaultViewModel,
                value ? size : 0L,
                cancellationToken);

            if (!result)
                return;

            IsRecycleBinEnabled = value;
            if (IsRecycleBinEnabled)
                _recycleBin ??= await RecycleBinService.TryGetOrCreateRecycleBinAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);
        }

        /// <summary>
        /// Updates the size bar occupied size and forces a recalculation, if necessary.
        /// </summary>
        /// <param name="forceRecalculation">Determines whether to recalculate item sizes in the recycle bin.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        public async Task UpdateSizesAsync(bool forceRecalculation, CancellationToken cancellationToken = default)
        {
            _recycleBin ??= await RecycleBinService.TryGetRecycleBinAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);
            if (_recycleBin is null)
                return;

            if (forceRecalculation)
                await RecycleBinService.TryRecalculateSizesAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);

            _occupiedSize = await _recycleBin.GetSizeAsync(cancellationToken);
            if (CurrentSizeOption is not null)
                UpdateSizeBar(CurrentSizeOption);
        }

        partial void OnCurrentSizeOptionChanged(PickerOptionViewModel? value)
        {
            UpdateSizeBar(value);
        }

        [RelayCommand]
        private void ToggleSelection(bool? value = null)
        {
            IsSelecting = value ?? !IsSelecting;
            Items.UnselectAll();
        }

        [RelayCommand]
        private void SelectAll()
        {
            IsSelecting = true;
            Items.SelectAll();
        }

        [RelayCommand]
        private void ClearSelection()
        {
            IsSelecting = true;
            Items.UnselectAll();
        }

        private void UpdateSizeBar(PickerOptionViewModel? value)
        {
            if (value is not null && value.Id != "-1")
            {
                var totalSize = long.Parse(value.Id);
                PercentageTaken = (double)_occupiedSize / totalSize * 100d;
                SpaceTakenText = $"Taken {ByteSize.FromBytes(_occupiedSize).ToBinaryString()} out of {ByteSize.FromBytes(totalSize).ToBinaryString()}";
            }
            else
                SpaceTakenText = null;
        }
    }
}