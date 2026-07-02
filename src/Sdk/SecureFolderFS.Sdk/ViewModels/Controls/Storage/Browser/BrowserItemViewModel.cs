using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Recyclable;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<IFileExplorerService>, Inject<IOverlayService>, Inject<IRecycleBinService>, Inject<IShareService>]
    [Bindable(true)]
    public abstract partial class BrowserItemViewModel : StorageItemViewModel, IAsyncInitialize
    {
        [ObservableProperty] private string? _SizeText;
        [ObservableProperty] private DateTime? _LastModified;

        /// <summary>
        /// Gets the <see cref="Views.Vault.BrowserViewModel"/> instance, which this item belongs to.
        /// </summary>
        public BrowserViewModel BrowserViewModel { get; }

        /// <summary>
        /// Gets the parent <see cref="FolderViewModel"/> that this item resides in, if any.
        /// </summary>
        public FolderViewModel? ParentFolder { get; }

        protected BrowserItemViewModel(BrowserViewModel browserViewModel, FolderViewModel? parentFolder)
        {
            ServiceProvider = DI.Default;
            BrowserViewModel = browserViewModel;
            ParentFolder = parentFolder;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the <see cref="BrowserItemViewModel.Inner"/> storable instance of this item.
        /// </summary>
        /// <param name="storable">The new storable object to use.</param>
        protected abstract void UpdateStorable(IStorable storable);

        /// <inheritdoc/>
        protected override void IsSelectedChanged(bool newValue)
        {
            if (newValue)
                ParentFolder?.SelectedItems.Add(this);
            else
                ParentFolder?.SelectedItems.Remove(this);
        }

        [RelayCommand]
        protected virtual async Task OpenInExternalAppAsync(CancellationToken cancellationToken)
        {
            if (Inner is not IFile file)
                return;

            await ShareService.OpenFileWithAsync(file);
        }

        [RelayCommand]
        protected virtual async Task OpenPropertiesAsync(CancellationToken cancellationToken)
        {
            var propertiesOverlay = new PropertiesOverlayViewModel(Inner);
            _ = propertiesOverlay.InitAsync(cancellationToken);

            await OverlayService.ShowAsync(propertiesOverlay);
        }

        [RelayCommand]
        protected virtual async Task MoveAsync(CancellationToken cancellationToken)
        {
            if (BrowserViewModel.Options.IsReadOnly)
                return;

            if (ParentFolder is null)
                return;

            if (BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel || ParentFolder.Folder is not IModifiableFolder modifiableParent)
                return;

            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            try
            {
                // Disable selection if called with selected items
                BrowserViewModel.IsSelecting = false;

                using var cts = transferViewModel.GetCancellation(cancellationToken);
                var destination = await transferViewModel.PickFolderAsync(new TransferOptions(TransferType.Move), false, cts.Token);
                if (destination is not IModifiableFolder destinationFolder)
                    return;

                // Workaround for the fact that the returned folder is IFolder and not FolderViewModel
                var destinationViewModel = BrowserViewModel.CurrentFolder;
                if (destinationViewModel is null || destinationViewModel.Inner.Id != destination.Id)
                    return;

                if (items.Any(item => IsAncestorOrSelf(destination.Id, item.Inner.Id)))
                    return;

                // Ensure the destination has content already loaded before collision checks
                if (destinationViewModel.Items.IsEmpty())
                    await destinationViewModel.ListContentsAsync(cts.Token);

                await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (item, reporter, token) =>
                {
                    // Check if the item source is the same as destination
                    if (destinationViewModel.Items.Any(x => x.Inner.Id == item.Id))
                        return;

                    // Get available name to avoid collision
                    var availableName = CollisionHelpers.GetAvailableName(item.Name, destinationViewModel.Items.Select(x => x.Inner.Name));

                    // Move
                    var movedItem = await destinationFolder.MoveStorableFromAsync(item, modifiableParent, false, availableName, reporter, token);

                    // Remove existing from folder
                    ParentFolder.Items.RemoveMatch(x => x.Inner.Id == item.Id)?.Dispose();

                    // Add to destination
                    destinationViewModel.Items.Insert(movedItem switch
                    {
                        IFile file => new FileViewModel(file, BrowserViewModel, destinationViewModel),
                        IFolder folder => new FolderViewModel(folder, ParentFolder.BrowserViewModel, destinationViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(movedItem))
                    }, BrowserViewModel.Layouts.GetSorter());
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                await transferViewModel.HideAsync();
            }
        }

        [RelayCommand]
        protected virtual async Task CopyAsync(CancellationToken cancellationToken)
        {
            if (BrowserViewModel.Options.IsReadOnly)
                return;

            if (ParentFolder is null)
                return;

            if (BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            try
            {
                // Disable selection, if called with selected items
                BrowserViewModel.IsSelecting = false;

                using var cts = transferViewModel.GetCancellation(cancellationToken);
                var destination = await transferViewModel.PickFolderAsync(new TransferOptions(TransferType.Copy), false, cts.Token);
                if (destination is not IModifiableFolder modifiableDestination)
                    return;

                // Workaround for the fact that the returned folder is IFolder and not FolderViewModel
                var destinationViewModel = BrowserViewModel.CurrentFolder;
                if (destinationViewModel is null || destinationViewModel.Inner.Id != destination.Id)
                    return;

                if (items.Any(item => IsAncestorOrSelf(destination.Id, item.Inner.Id)))
                    return;

                // Ensure the destination has content already loaded before collision checks
                if (destinationViewModel.Items.IsEmpty())
                    await destinationViewModel.ListContentsAsync(cts.Token);

                await transferViewModel.TransferAsync(items.Select(x => x.Inner), async (item, reporter, token) =>
                {
                    // Get available name to avoid collision
                    var availableName = CollisionHelpers.GetAvailableName(item.Name, destinationViewModel.Items.Select(x => x.Inner.Name));

                    // Copy
                    var copiedItem = await modifiableDestination.CreateCopyOfStorableAsync(item, false, availableName, reporter, token);

                    // Add to destination
                    destinationViewModel.Items.Insert(copiedItem switch
                    {
                        IFile file => new FileViewModel(file, BrowserViewModel, destinationViewModel),
                        IFolder folder => new FolderViewModel(folder, BrowserViewModel, destinationViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(copiedItem))
                    }, BrowserViewModel.Layouts.GetSorter());
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                await transferViewModel.HideAsync();
            }
        }

        [RelayCommand]
        protected virtual async Task RenameAsync(CancellationToken cancellationToken)
        {
            if (BrowserViewModel.Options.IsReadOnly)
                return;

            if (ParentFolder?.Folder is not IRenamableFolder renamableFolder)
                return;

            if (Inner is not IStorableChild innerChild)
                return;

            try
            {
                var viewModel = new RenameOverlayViewModel("Rename".ToLocalized()) { Message = "ChooseNewName".ToLocalized(), NewName = Inner.Name };
                var result = await OverlayService.ShowAsync(viewModel);
                if (!result.Positive())
                    return;

                if (string.IsNullOrWhiteSpace(viewModel.NewName))
                    return;

                var formattedName = FormattingHelpers.SanitizeItemName(viewModel.NewName, "Renamed item");
                if (!Path.HasExtension(formattedName))
                    formattedName = $"{formattedName}{Path.GetExtension(innerChild.Name)}";

                var existingItem = await renamableFolder.TryGetFirstByNameAsync(formattedName, cancellationToken);
                if (existingItem is not null)
                {
                    await OverlayService.ShowAsync(new MessageOverlayViewModel()
                    {
                        Title = "InvalidItemName".ToLocalized(),
                        Message = "ItemAlreadyExists".ToLocalized(formattedName),
                        SecondaryText = "Close".ToLocalized()
                    });
                    return;
                }

                var renamedStorable = await renamableFolder.RenameAsync(innerChild, formattedName, cancellationToken);
                Title = formattedName;
                UpdateStorable(renamedStorable);
                _ = InitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // TODO: Report error
                _ = ex;
            }
        }

        [RelayCommand]
        protected virtual async Task DeleteAsync(CancellationToken cancellationToken)
        {
            if (BrowserViewModel.Options.IsReadOnly)
                return;

            if (ParentFolder is null)
                return;

            if (BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            // Disable selection, if called with selected items
            BrowserViewModel.IsSelecting = false;

            try
            {
                transferViewModel.TransferType = TransferType.Delete;
                using var cts = transferViewModel.GetCancellation(cancellationToken);

                if (BrowserViewModel.Options.IsRecycleBinEnabled() && BrowserViewModel.StorageRoot is not null)
                {
                    if (ParentFolder?.Folder is not IRecyclableFolder recyclableFolder)
                        return;

                    var recycleBin = await RecycleBinService.TryGetOrCreateRecycleBinAsync(BrowserViewModel.StorageRoot, cts.Token);
                    if (recycleBin is null)
                        return;

                    // Key the sizes by item ID - the transfer callback receives the storable,
                    // so positional lookups against the view model array would not match
                    var sizes = new Dictionary<string, long>();
                    foreach (var item in items)
                    {
                        sizes[item.Inner.Id] = item.Inner switch
                        {
                            IFile file => await file.GetSizeAsync(cts.Token) ?? 0L,
                            IFolder folder => await folder.GetSizeAsync(cts.Token) ?? 0L,
                            _ => 0L
                        };
                    }

                    if (BrowserViewModel.Options.IsRecycleBinUnlimited())
                    {
                        await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (item, token) =>
                        {
                            await recyclableFolder.DeleteAsync(item, sizes.GetValueOrDefault(item.Id, 0L), false, token);

                            var itemToRemove = ParentFolder.Items.FirstOrDefault(x => x.Inner.Id == item.Id);
                            if (itemToRemove is not null)
                                ParentFolder.Items.RemoveAndGet(itemToRemove)?.Dispose();
                        }, cts.Token);
                    }
                    else
                    {
                        var occupiedSize = await recycleBin.GetSizeAsync(cts.Token);
                        var availableSize = BrowserViewModel.Options.RecycleBinSize - occupiedSize;
                        var permanently = availableSize < sizes.Values.Sum();

                        if (permanently)
                        {
                            var messageOverlay = new MessageOverlayViewModel()
                            {
                                Title = "NotEnoughSpace".ToLocalized(),
                                Message = "ItemsExceedRecycleBinSizePlural".ToLocalized(items.Length),
                                PrimaryText = "Delete".ToLocalized(),
                                SecondaryText = "Cancel".ToLocalized()
                            };

                            var result = await OverlayService.ShowAsync(messageOverlay);
                            if (!result.Positive())
                                return;
                        }

                        await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (item, token) =>
                        {
                            await recyclableFolder.DeleteAsync(item, sizes.GetValueOrDefault(item.Id, 0L), permanently, token);

                            var itemToRemove = ParentFolder.Items.FirstOrDefault(x => x.Inner.Id == item.Id);
                            if (itemToRemove is not null)
                                ParentFolder.Items.RemoveAndGet(itemToRemove)?.Dispose();
                        }, cts.Token);
                    }
                }
                else
                {
                    if (ParentFolder?.Folder is not IModifiableFolder modifiableFolder)
                        return;

                    var messageOverlay = new MessageOverlayViewModel()
                    {
                        Title = "ItemDeletionTitle".ToLocalized(),
                        Message = "ItemDeletionDescriptionPlural".ToLocalized(items.Length),
                        PrimaryText = "Delete".ToLocalized(),
                        SecondaryText = "Cancel".ToLocalized()
                    };

                    var result = await OverlayService.ShowAsync(messageOverlay);
                    if (!result.Positive())
                        return;

                    await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (item, token) =>
                    {
                        await modifiableFolder.DeleteAsync(item, deleteImmediately: true, cancellationToken: token);

                        var itemToRemove = ParentFolder.Items.FirstOrDefault(x => x.Inner.Id == item.Id);
                        if (itemToRemove is not null)
                            ParentFolder.Items.RemoveAndGet(itemToRemove)?.Dispose();
                    }, cts.Token);
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                await transferViewModel.HideAsync();
            }
        }

        [RelayCommand]
        protected virtual async Task ExportAsync(CancellationToken cancellationToken)
        {
            if (ParentFolder is null)
                return;

            if (BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            var destination = await FileExplorerService.PickFolderAsync(null, false, cancellationToken);
            if (destination is not IModifiableFolder destinationFolder)
                return;

            // Sources in read-only vaults cannot be removed - export becomes a copy
            var removeSource = !BrowserViewModel.Options.IsReadOnly && ParentFolder.Folder is IModifiableFolder;

            try
            {
                transferViewModel.TransferType = removeSource ? TransferType.Move : TransferType.Copy;
                using var cts = transferViewModel.GetCancellation(cancellationToken);
                await transferViewModel.TransferAsync(items.Select(x => x.Inner), async (item, reporter, token) =>
                {
                    // Copy and delete
                    await destinationFolder.CreateCopyOfStorableAsync(item, false, reporter, token);
                    if (removeSource && ParentFolder.Folder is IModifiableFolder parentModifiableFolder)
                    {
                        await parentModifiableFolder.DeleteAsync((IStorableChild)item, token);
                        ParentFolder.Items.RemoveMatch(x => x.Inner.Id == item.Id)?.Dispose();
                    }
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                await transferViewModel.HideAsync();
            }
        }

        [RelayCommand]
        protected abstract Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Determines whether <paramref name="destinationId"/> points to <paramref name="itemId"/> itself
        /// or to a descendant of it, respecting path segment boundaries.
        /// </summary>
        /// <remarks>
        /// A plain substring check would misclassify sibling paths that share a prefix (e.g. '/a/bc' and '/a/b').
        /// </remarks>
        private static bool IsAncestorOrSelf(string destinationId, string itemId)
        {
            if (destinationId.Equals(itemId, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!destinationId.StartsWith(itemId, StringComparison.OrdinalIgnoreCase))
                return false;

            // The prefix must end exactly at a path separator boundary
            return itemId.EndsWith('/') || itemId.EndsWith('\\')
                   || destinationId[itemId.Length] is '/' or '\\';
        }
    }
}
