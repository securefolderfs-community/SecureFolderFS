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
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IRecycleBinService>, Inject<ISystemService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize, IProgress<IResult>, IDisposable
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
        [ObservableProperty] private InfoBarViewModel _StatusInfoBar = new();

        public INavigator OuterNavigator { get; }

        public bool IsInitialized { get; private set; }

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

            // Choose the saved size option. If the configured size does not match any of the
            // generated options (e.g., the vault was configured on a device with more free space),
            // a custom option is added so that toggling the recycle bin never silently rewrites
            // the user's configured quota
            var configuredSize = UnlockedVaultViewModel.StorageRoot.Options.RecycleBinSize;
            var matchedOption = SizeOptions.FirstOrDefault(x => long.Parse(x.Id) == configuredSize);
            if (matchedOption is null && configuredSize > 0L)
            {
                matchedOption = new(configuredSize.ToString(), ByteSize.FromBytes(configuredSize).ToBinaryString());
                SizeOptions.Add(matchedOption);
            }

            CurrentSizeOption = matchedOption
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
                _occupiedSize = await _recycleBin.GetSizeAsync(cancellationToken) ?? 0L;
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

            IsInitialized = true;
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

            _occupiedSize = await _recycleBin.GetSizeAsync(cancellationToken) ?? 0L;
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

            var itemsToRestore = items.Select(x => x.AsWrapper<IStorable>().GetWrapperAt(1).Inner).Cast<IStorableChild>().ToArray();
            var folderPicker = DI.Service<IFileExplorerService>();
            if (await _recycleBin.TryRestoreItemsAsync(itemsToRestore, folderPicker, cancellationToken))
            {
                StatusInfoBar.IsOpen = false;
                foreach (var item in items)
                    Items.Remove(item);

                ToggleSelectionCommand.Execute(false);
                await UpdateSizesAsync(false, cancellationToken);
            }
            else
            {
                Report(new MessageResult(false, "ItemsFailedToRestorePlural".ToLocalized(items.Length)));

                // Some items may have been restored before the failure - refresh the
                // listing so the view matches the on-disk state
                ToggleSelectionCommand.Execute(false);
                await InitAsync(cancellationToken);
            }
        }

        [RelayCommand]
        private async Task DeleteSelectedAsync(IList<object>? selectedItems, CancellationToken cancellationToken)
        {
            if (_recycleBin is null || selectedItems is null || selectedItems.Count == 0)
                return;

            var items = selectedItems.OfType<RecycleBinItemViewModel>().ToArray();
            if (items.Length == 0)
                return;

            var failedCount = 0;
            Exception? lastException = null;
            foreach (var item in items)
            {
                if (item.AsWrapper<IStorable>().GetWrapperAt(1).Inner is not IStorableChild innerChild)
                    continue;

                try
                {
                    await _recycleBin.DeleteAsync(innerChild, cancellationToken);
                    Items.Remove(item);
                }
                catch (Exception ex)
                {
                    // A failed item must not abandon the remaining ones - aggregate and report once
                    failedCount++;
                    lastException = ex;
                }
            }

            if (failedCount > 0)
                Report(Result.Failure(lastException));
            else
                StatusInfoBar.IsOpen = false;

            ToggleSelectionCommand.Execute(false);
            await UpdateSizesAsync(false, cancellationToken);
        }

        /// <inheritdoc/>
        public void Report(IResult result)
        {
            StatusInfoBar.Title = "ErrorOccurred".ToLocalized();
            StatusInfoBar.Message = result.GetMessage(result.Exception?.Message ?? "UnknownError".ToLocalized());
            StatusInfoBar.Severity = Severity.Critical;
            StatusInfoBar.IsCloseable = true;
            StatusInfoBar.IsOpen = true;
        }

        private void UpdateSizeBar(PickerOptionViewModel? value)
        {
            if (value is not null && value.Id != "-1" && long.Parse(value.Id) is > 0L and var totalSize)
            {
                PercentageTaken = Math.Clamp((double)_occupiedSize / totalSize * 100d, 0d, 100d);
                SpaceTakenText = "RecycleBinSpaceTaken".ToLocalized(ByteSize.FromBytes(_occupiedSize).ToBinaryString(), ByteSize.FromBytes(totalSize).ToBinaryString());
            }
            else
                SpaceTakenText = null;
        }

        private async void FolderWatcher_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            await _synchronizationContext.PostOrExecuteAsync(async () => await UpdateCollectionAsync());
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
                                if (Items.Any(x => x.AsWrapper<IStorable>().GetWrapperAt(1).Inner.Id == newItem.Id))
                                    continue;

                                // The configuration file is written before the payload is moved, so the
                                // item is materializable as soon as its event arrives. Core files (e.g.
                                // the occupied-size file, which changes on every delete) resolve to null
                                if (await _recycleBin.TryGetItemAsync(newItem.Name) is IRecycleBinItem recycleBinItem)
                                    Items.Add(new RecycleBinItemViewModel(this, recycleBinItem, _recycleBin).WithInitAsync());
                            }

                            // Update size bar after changes
                            await UpdateSizesAsync(false);
                            break;
                        }

                        case NotifyCollectionChangedAction.Remove when e.OldItems is not null:
                        {
                            foreach (var oldItem in e.OldItems.OfType<IStorable>())
                            {
                                var existingItem = Items.FirstOrDefault(x => x.AsWrapper<IStorable>().GetWrapperAt(1).Inner.Id == oldItem.Id);
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
            _folderWatcher?.CollectionChanged -= FolderWatcher_CollectionChanged;
            _folderWatcher?.Dispose();
        }
    }
}