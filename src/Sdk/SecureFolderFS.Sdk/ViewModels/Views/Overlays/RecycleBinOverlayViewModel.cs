using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IRecycleBinService>, Inject<ISystemService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize, IDisposable
    {
        private readonly SynchronizationContext? _synchronizationContext;
        private long _occupiedSize;
        private IRecycleBinFolder? _recycleBin;
        private IFolderWatcher? _folderWatcher;

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
            _synchronizationContext = SynchronizationContext.Current;
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
            var rootFolder = UnlockedVaultViewModel.VaultFolder;
            if (rootFolder is IChildFolder childFolder)
                rootFolder = await childFolder.GetRootAsync(cancellationToken) ?? childFolder;

            // Get and populate available size options
            var deviceFreeSpace = await SafetyHelpers.NoFailureAsync(async () => await SystemService.GetAvailableFreeSpaceAsync(rootFolder, cancellationToken));
            var sizeOptions = RecycleBinHelpers.GetSizeOptions(deviceFreeSpace);
            SizeOptions.Clear();
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
            Items.Clear();
            await foreach (var item in _recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item is not IRecycleBinItem recycleBinItem)
                    continue;

                Items.Add(new RecycleBinItemViewModel(this, recycleBinItem, _recycleBin).WithInitAsync(cancellationToken));
            }

            // Dispose the old folder watcher if it exists
            if (_folderWatcher is not null)
            {
                _folderWatcher.CollectionChanged -= FolderWatcher_CollectionChanged;
                _folderWatcher.Dispose();
            }

            // Set up a folder watcher to dynamically update the collection
            _folderWatcher = await _recycleBin.TryGetFolderWatcherAsync(cancellationToken);
            if (_folderWatcher is not null)
                _folderWatcher.CollectionChanged += FolderWatcher_CollectionChanged;
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
        private async Task RefreshAsync(CancellationToken cancellationToken)
        {
            await InitAsync(cancellationToken);
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

        [RelayCommand]
        private async Task RestoreSelectedAsync(IList<object>? selectedItems, CancellationToken cancellationToken)
        {
            if (_recycleBin is null || selectedItems is null || selectedItems.Count == 0)
                return;

            var items = selectedItems.OfType<RecycleBinItemViewModel>().ToArray();
            if (items.Length == 0)
                return;

            var folderPicker = DI.Service<IFileExplorerService>();
            if (await _recycleBin.TryRestoreItemsAsync(items.Select(x => x.Inner as IStorableChild)!, folderPicker, cancellationToken))
            {
                foreach (var item in items)
                    Items.Remove(item);
            }

            ToggleSelectionCommand.Execute(false);
            await UpdateSizesAsync(false, cancellationToken);
        }

        [RelayCommand]
        private async Task DeleteSelectedAsync(IList<object>? selectedItems, CancellationToken cancellationToken)
        {
            if (_recycleBin is null || selectedItems is null || selectedItems.Count == 0)
                return;

            var items = selectedItems.OfType<RecycleBinItemViewModel>().ToArray();
            if (items.Length == 0)
                return;

            foreach (var item in items)
            {
                if (item.Inner is not IStorableChild innerChild)
                    continue;

                try
                {
                    await _recycleBin.DeleteAsync(innerChild, cancellationToken);
                    Items.Remove(item);
                }
                catch (Exception ex)
                {
                    _ = ex;
                }
            }

            ToggleSelectionCommand.Execute(false);
            await UpdateSizesAsync(false, cancellationToken);
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

        private async void FolderWatcher_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            await _synchronizationContext.PostOrExecuteAsync(async _ => await UpdateCollectionAsync());
            return;

            async Task UpdateCollectionAsync()
            {
                if (_recycleBin is null)
                    return;

                try
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add when e.NewItems is not null:
                        {
                            foreach (var newItem in e.NewItems.OfType<IStorable>())
                            {
                                // Skip configuration files
                                if (newItem.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                // Skip if item already exists in collection
                                if (Items.Any(x => x.Inner.Id == newItem.Id))
                                    continue;

                                await Task.Delay(200); // Delay to ensure the item is fully available

                                // Get the recycle bin item wrapper
                                await foreach (var item in _recycleBin.GetItemsAsync())
                                {
                                    if (item is not IRecycleBinItem recycleBinItem || recycleBinItem.Inner.Id != newItem.Id)
                                        continue;

                                    Items.Add(new RecycleBinItemViewModel(this, recycleBinItem, _recycleBin).WithInitAsync());
                                    break;
                                }
                            }

                            // Update size bar after changes
                            await UpdateSizesAsync(false);
                            break;
                        }

                        case NotifyCollectionChangedAction.Remove when e.OldItems is not null:
                        {
                            foreach (var oldItem in e.OldItems.OfType<IStorable>())
                            {
                                var existingItem = Items.FirstOrDefault(x => x.Inner.Id == oldItem.Id);
                                if (existingItem is not null)
                                    Items.Remove(existingItem);
                            }

                            // Update size bar after changes
                            await UpdateSizesAsync(false);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_folderWatcher is not null)
            {
                _folderWatcher.CollectionChanged -= FolderWatcher_CollectionChanged;
                _folderWatcher.Dispose();
            }
        }
    }
}