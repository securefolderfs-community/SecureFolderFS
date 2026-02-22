using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<IFileExplorerService>, Inject<IOverlayService>, Inject<IRecycleBinService>]
    [Bindable(true)]
    public abstract partial class BrowserItemViewModel : StorageItemViewModel, IAsyncInitialize
    {
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

        [RelayCommand]
        protected virtual async Task OpenPropertiesAsync(CancellationToken cancellationToken)
        {
            if (Inner is not IStorableProperties storableProperties)
                return;

            var properties = await storableProperties.GetPropertiesAsync();
            var propertiesOverlay = new PropertiesOverlayViewModel(Inner, properties);
            _ = propertiesOverlay.InitAsync(cancellationToken);

            await OverlayService.ShowAsync(propertiesOverlay);
        }

        [RelayCommand]
        protected virtual async Task MoveAsync(CancellationToken cancellationToken)
        {
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
                // TODO: Check consequences of this where the CurrentFolder might differ from the actual picked folder
                var destinationViewModel = BrowserViewModel.CurrentFolder;
                if (destinationViewModel is null)
                    return;

                if (items.Any(item => destination.Id.Contains(item.Inner.Id, StringComparison.InvariantCultureIgnoreCase)))
                    return;

                await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (item, reporter, token) =>
                {
                    // Move
                    var movedItem = await destinationFolder.MoveStorableFromAsync(item, modifiableParent, false, reporter, token);

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
                // TODO: Check consequences of this where the CurrentFolder might differ from the actual picked folder
                var destinationViewModel = BrowserViewModel.CurrentFolder;
                if (destinationViewModel is null)
                    return;

                if (items.Any(item => destination.Id.Contains(item.Inner.Id, StringComparison.InvariantCultureIgnoreCase)))
                    return;

                await transferViewModel.TransferAsync(items.Select(x => x.Inner), async (item, reporter, token) =>
                {
                    // Copy
                    var copiedItem = await modifiableDestination.CreateCopyOfStorableAsync(item, false, reporter, token);

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

                var formattedName = CollisionHelpers.GetAvailableName(
                    FormattingHelpers.SanitizeItemName(viewModel.NewName, "Renamed item"),
                    ParentFolder.Items.Select(x => x.Inner.Name));

                if (!Path.HasExtension(formattedName))
                    formattedName = $"{formattedName}{Path.GetExtension(innerChild.Name)}";

                var existingItem = await renamableFolder.TryGetFirstByNameAsync(formattedName, cancellationToken);
                if (existingItem is not null)
                {
                    // TODO: Report that the item already exists
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

                    var sizes = new List<long>();
                    foreach (var item in items)
                    {
                        sizes.Add(item.Inner switch
                        {
                            IFile file => await file.GetSizeAsync(cts.Token),
                            IFolder folder => await folder.GetSizeAsync(cts.Token),
                            _ => 0L
                        });
                    }

                    if (BrowserViewModel.Options.IsRecycleBinUnlimited())
                    {
                        await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (item, token) =>
                        {
                            var idx = Array.IndexOf(items, item);
                            await recyclableFolder.DeleteAsync(item, sizes[idx], false, token);

                            var itemToRemove = ParentFolder.Items.FirstOrDefault(x => x.Inner.Id == item.Id);
                            if (itemToRemove is not null)
                                ParentFolder.Items.RemoveAndGet(itemToRemove)?.Dispose();
                        }, cts.Token);
                    }
                    else
                    {
                        var occupiedSize = await recycleBin.GetSizeAsync(cts.Token);
                        var availableSize = BrowserViewModel.Options.RecycleBinSize - occupiedSize;
                        var permanently = availableSize < sizes.Sum();

                        if (permanently)
                        {
                            var messageOverlay = new MessageOverlayViewModel()
                            {
                                Title = "NotEnoughSpace".ToLocalized(),
                                Message = "ItemsExceedRecycleBinSize".ToLocalized(items.Length),
                                PrimaryText = "Delete".ToLocalized(),
                                SecondaryText = "Cancel".ToLocalized()
                            };

                            var result = await OverlayService.ShowAsync(messageOverlay);
                            if (!result.Positive())
                                return;
                        }

                        var idx = 0;
                        await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (item, token) =>
                        {
                            await recyclableFolder.DeleteAsync(item, sizes[idx], permanently, token);

                            var itemToRemove = ParentFolder.Items.FirstOrDefault(x => x.Inner.Id == item.Id);
                            if (itemToRemove is not null)
                                ParentFolder.Items.RemoveAndGet(itemToRemove)?.Dispose();

                            idx++;
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
                        Message = "ItemDeletionDescription".ToLocalized(items.Length),
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
            if (ParentFolder?.Folder is not IModifiableFolder parentModifiableFolder)
                return;

            if (BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            var destination = await FileExplorerService.PickFolderAsync(null, false, cancellationToken);
            if (destination is not IModifiableFolder destinationFolder)
                return;

            transferViewModel.TransferType = TransferType.Move;
            using var cts = transferViewModel.GetCancellation(cancellationToken);
            await transferViewModel.TransferAsync(items.Select(x => x.Inner), async (item, reporter, token) =>
            {
                // Copy and delete
                await destinationFolder.CreateCopyOfStorableAsync(item, false, reporter, token);
                await parentModifiableFolder.DeleteAsync((IStorableChild)item, token);

                ParentFolder.Items.RemoveMatch(x => x.Inner.Id == item.Id)?.Dispose();
            }, cts.Token);
        }

        [RelayCommand]
        protected abstract Task OpenAsync(CancellationToken cancellationToken);
    }
}
