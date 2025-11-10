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
                // Disable selection, if called with selected items
                BrowserViewModel.IsSelecting = false;

                using var cts = transferViewModel.GetCancellation();
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

                using var cts = transferViewModel.GetCancellation();
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

            // TODO: If moving to trash, show TransferViewModel (with try..catch..finally), otherwise don't show anything
            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            // Disable selection, if called with selected items
            BrowserViewModel.IsSelecting = false;

            if (BrowserViewModel.Options.IsRecycleBinEnabled() && BrowserViewModel.StorageRoot is not null)
            {
                if (ParentFolder?.Folder is not IRecyclableFolder recyclableFolder)
                    return;

                var recycleBin = await RecycleBinService.TryGetOrCreateRecycleBinAsync(BrowserViewModel.StorageRoot, cancellationToken);
                if (recycleBin is null)
                    return;

                var sizes = new List<long>();
                foreach (var item in items)
                {
                    sizes.Add(item.Inner switch
                    {
                        IFile file => await file.GetSizeAsync(cancellationToken),
                        IFolder folder => await folder.GetSizeAsync(cancellationToken),
                        _ => 0L
                    });
                }

                if (BrowserViewModel.Options.IsRecycleBinUnlimited())
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        await recyclableFolder.DeleteAsync((IStorableChild)item.Inner, sizes[i], false, cancellationToken);
                        ParentFolder?.Items.RemoveAndGet(item)?.Dispose();
                    }
                }
                else
                {
                    var occupiedSize = await recycleBin.GetSizeAsync(cancellationToken);
                    var availableSize = BrowserViewModel.Options.RecycleBinSize - occupiedSize;
                    if (availableSize < sizes.Sum())
                    {
                        // TODO: Show an overlay telling the user there's not enough space and the items will be deleted permanently
                        for (var i = 0; i < items.Length; i++)
                        {
                            var item = items[i];
                            await recyclableFolder.DeleteAsync((IStorableChild)item.Inner, sizes[i], true, cancellationToken);
                            ParentFolder.Items.RemoveAndGet(item)?.Dispose();
                        }
                    }
                    else
                    {
                        for (var i = 0; i < items.Length; i++)
                        {
                            var item = items[i];
                            await recyclableFolder.DeleteAsync((IStorableChild)item.Inner, sizes[i], false, cancellationToken);
                            ParentFolder.Items.RemoveAndGet(item)?.Dispose();
                        }
                    }
                }
            }
            else
            {
                if (ParentFolder?.Folder is not IModifiableFolder modifiableFolder)
                    return;

                // TODO: Show an overlay to ask the user **when deleting permanently**

                foreach (var item in items)
                {
                    await modifiableFolder.DeleteAsync((IStorableChild)item.Inner, cancellationToken);
                    ParentFolder.Items.RemoveAndGet(item)?.Dispose();
                }
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
            using var cts = transferViewModel.GetCancellation();
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
