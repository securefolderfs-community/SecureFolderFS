﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Inject<IFileExplorerService>, Inject<IOverlayService>]
    [Bindable(true)]
    public abstract partial class BrowserItemViewModel : ObservableObject, IWrapper<IStorable>, IViewable, IAsyncInitialize
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsSelected;
        [ObservableProperty] private IImage? _Thumbnail;

        /// <inheritdoc/>
        public abstract IStorable Inner { get; }

        /// <summary>
        /// Gets the parent <see cref="FolderViewModel"/> that this item resides in, if any.
        /// </summary>
        public FolderViewModel? ParentFolder { get; }

        protected BrowserItemViewModel(FolderViewModel? parentFolder)
        {
            ServiceProvider = DI.Default;
            ParentFolder = parentFolder;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        protected abstract void UpdateStorable(IStorable storable);

        [RelayCommand]
        protected virtual async Task MoveAsync(CancellationToken cancellationToken)
        {
            var items = ParentFolder?.GetSelectedItems().ToArray();
            if (items?.IsEmpty() ?? true)
                items = [ this ];
            
            if (ParentFolder?.BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel || ParentFolder?.Folder is not IModifiableFolder modifiableParent)
                return;

            try
            {
                // Disable selection, if called with selected items
                ParentFolder.BrowserViewModel.IsSelecting = false;
                
                using var cts = new CancellationTokenSource();
                var destination = await transferViewModel.SelectFolderAsync(TransferType.Move, cts);
                if (destination is not { Folder: IModifiableFolder destinationFolder })
                    return;
                
                foreach (var item in items)
                {
                    if (destination.Folder.Id.Contains(item.Inner.Id, StringComparison.InvariantCultureIgnoreCase))
                        return;
                }

                await transferViewModel.TransferAsync(items.Select(x => (IStorableChild)x.Inner), async (storable, token) =>
                {
                    // Move
                    var movedItem = await destinationFolder.MoveStorableFromAsync(storable, modifiableParent, false, token);
                    
                    // Remove existing from folder
                    ParentFolder.Items.RemoveMatch(x => x.Inner.Id == storable.Id);
                    
                    // Add to destination
                    destination.Items.Add(movedItem switch
                    {
                        IFile file => new FileViewModel(file, destination),
                        IFolder folder => new FolderViewModel(folder, ParentFolder.BrowserViewModel, destination),
                        _ => throw new ArgumentOutOfRangeException(nameof(movedItem))
                    });
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
            var items = ParentFolder?.GetSelectedItems().ToArray();
            if (items?.IsEmpty() ?? true)
                items = [ this ];
            
            if (ParentFolder?.BrowserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            try
            {
                // Disable selection, if called with selected items
                ParentFolder.BrowserViewModel.IsSelecting = false;

                using var cts = new CancellationTokenSource();
                var destination = await transferViewModel.SelectFolderAsync(TransferType.Copy, cts);
                if (destination is not { Folder: IModifiableFolder destinationFolder })
                    return;

                foreach (var item in items)
                {
                    if (destination.Folder.Id.Contains(item.Inner.Id, StringComparison.InvariantCultureIgnoreCase))
                        return;
                }

                await transferViewModel.TransferAsync(items.Select(x => x.Inner), async (storable, token) =>
                {
                    // Copy
                    var copiedItem = await destinationFolder.CreateCopyOfStorableAsync(storable, false, token);

                    // Add to destination
                    destination.Items.Add(copiedItem switch
                    {
                        IFile file => new FileViewModel(file, destination),
                        IFolder folder => new FolderViewModel(folder, ParentFolder.BrowserViewModel, destination),
                        _ => throw new ArgumentOutOfRangeException(nameof(copiedItem))
                    });
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
            
            var items = ParentFolder.GetSelectedItems().ToArray();
            if (items.IsEmpty())
                items = [this];
            
            // Disable selection, if called with selected items
            ParentFolder.BrowserViewModel.IsSelecting = false;

            foreach (var item in items)
            {
                await modifiableFolder.DeleteAsync((IStorableChild)item.Inner, cancellationToken);
                ParentFolder?.Items.Remove(item);
            }
        }

        [RelayCommand]
        protected virtual async Task ExportAsync(CancellationToken cancellationToken)
        {
            if (ParentFolder?.Folder is not IModifiableFolder parentModifiableFolder)
                return;
            
            var destination = await FileExplorerService.PickFolderAsync(false, cancellationToken);
            if (destination is not IModifiableFolder destinationFolder)
                return;

            // Copy and delete
            await destinationFolder.CreateCopyOfStorableAsync(Inner, false, cancellationToken);
            await parentModifiableFolder.DeleteAsync((IStorableChild)Inner, cancellationToken);
            
            ParentFolder.Items.Remove(this);
        }
        
        [RelayCommand]
        protected abstract Task OpenAsync(CancellationToken cancellationToken);
    }
}
