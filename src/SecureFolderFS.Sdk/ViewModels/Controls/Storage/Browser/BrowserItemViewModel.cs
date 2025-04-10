using System;
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
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<IFileExplorerService>, Inject<IOverlayService>]
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

            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            if (BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel || ParentFolder.Folder is not IModifiableFolder modifiableParent)
                return;

            try
            {
                // Disable selection, if called with selected items
                BrowserViewModel.IsSelecting = false;

                using var cts = transferViewModel.GetCancellation();
                var destination = await transferViewModel.PickFolderAsync(new TransferFilter(TransferType.Move), false, cts.Token);
                if (destination is not IModifiableFolder destinationFolder)
                    return;

                // Workaround for the fact that the returned folder is IFolder and not FolderViewModel
                // TODO: Check consequences of this where the CurrentFolder might differ from the actual picked folder
                var destinationViewModel = BrowserViewModel.CurrentFolder;
                if (destinationViewModel is null)
                    return;

                if (items.Any(item => destination.Id.Contains(item.Inner.Id, StringComparison.InvariantCultureIgnoreCase)))
                    return;

                await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (storable, token) =>
                {
                    // Move
                    var movedItem = await destinationFolder.MoveStorableFromAsync(storable, modifiableParent, false, token);

                    // Remove existing from folder
                    ParentFolder.Items.RemoveMatch(x => x.Inner.Id == storable.Id)?.Dispose();

                    // Add to destination
                    destinationViewModel.Items.Insert(movedItem switch
                    {
                        IFile file => new FileViewModel(file, BrowserViewModel, destinationViewModel),
                        IFolder folder => new FolderViewModel(folder, ParentFolder.BrowserViewModel, destinationViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(movedItem))
                    }, BrowserViewModel.ViewOptions.GetSorter());
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                transferViewModel.IsVisible = false;
                transferViewModel.IsProgressing = false;
            }
        }

        [RelayCommand]
        protected virtual async Task CopyAsync(CancellationToken cancellationToken)
        {
            if (ParentFolder is null)
                return;

            var items = BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [ this ];

            if (BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            try
            {
                // Disable selection, if called with selected items
                BrowserViewModel.IsSelecting = false;

                using var cts = transferViewModel.GetCancellation();
                var destination = await transferViewModel.PickFolderAsync(new TransferFilter(TransferType.Copy), false, cts.Token);
                if (destination is not IModifiableFolder modifiableDestination)
                    return;

                // Workaround for the fact that the returned folder is IFolder and not FolderViewModel
                // TODO: Check consequences of this where the CurrentFolder might differ from the actual picked folder
                var destinationViewModel = BrowserViewModel.CurrentFolder;
                if (destinationViewModel is null)
                    return;

                if (items.Any(item => destination.Id.Contains(item.Inner.Id, StringComparison.InvariantCultureIgnoreCase)))
                    return;

                await transferViewModel.TransferAsync(items.Select(x => x.Inner), async (storable, token) =>
                {
                    // Copy
                    var copiedItem = await modifiableDestination.CreateCopyOfStorableAsync(storable, false, token);

                    // Add to destination
                    destinationViewModel.Items.Insert(copiedItem switch
                    {
                        IFile file => new FileViewModel(file, BrowserViewModel, destinationViewModel),
                        IFolder folder => new FolderViewModel(folder, BrowserViewModel, destinationViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(copiedItem))
                    }, BrowserViewModel.ViewOptions.GetSorter());
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                transferViewModel.IsVisible = false;
                transferViewModel.IsProgressing = false;
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
                var viewModel = new RenameOverlayViewModel("Rename item") { Message = "Choose a new name" };
                var result = await OverlayService.ShowAsync(viewModel);
                if (!result.Positive())
                    return;

                if (string.IsNullOrWhiteSpace(viewModel.NewName))
                    return;

                var formattedName = FormattingHelpers.SanitizeItemName(viewModel.NewName, "item");
                if (!Path.HasExtension(formattedName))
                    formattedName = $"{formattedName}{Path.GetExtension(innerChild.Name)}";

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
            if (ParentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            // TODO: Show an overlay to ask the user **when deleting permanently**
            // TODO: If moving to trash, show TransferViewModel (with try..catch..finally), otherwise don't show anything

            var items = ParentFolder.BrowserViewModel.IsSelecting ? ParentFolder.Items.GetSelectedItems().ToArray() : [];
            if (items.IsEmpty())
                items = [this];

            // Disable selection, if called with selected items
            ParentFolder.BrowserViewModel.IsSelecting = false;

            foreach (var item in items)
            {
                await modifiableFolder.DeleteAsync((IStorableChild)item.Inner, cancellationToken);
                ParentFolder?.Items.RemoveAndGet(item)?.Dispose();
            }
        }

        [RelayCommand]
        protected virtual async Task ExportAsync(CancellationToken cancellationToken)
        {
            if (ParentFolder?.Folder is not IModifiableFolder parentModifiableFolder)
                return;

            var destination = await FileExplorerService.PickFolderAsync(null, false, cancellationToken);
            if (destination is not IModifiableFolder destinationFolder)
                return;

            // Copy and delete
            await destinationFolder.CreateCopyOfStorableAsync(Inner, false, cancellationToken);
            await parentModifiableFolder.DeleteAsync((IStorableChild)Inner, cancellationToken);

            ParentFolder.Items.RemoveAndGet(this)?.Dispose();
        }

        [RelayCommand]
        protected abstract Task OpenAsync(CancellationToken cancellationToken);
    }
}
