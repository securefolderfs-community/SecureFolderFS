using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Inject<IFileExplorerService>]
    [Bindable(true)]
    public abstract partial class BrowserItemViewModel : ObservableObject, IWrapper<IStorable>, IViewable, IAsyncInitialize
    {
        [ObservableProperty] private string? _Title;
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

        [RelayCommand]
        protected virtual async Task MoveAsync(CancellationToken cancellationToken)
        {
            if (Inner is not IStorableChild innerChild)
                return;
            
            if (ParentFolder?.TransferViewModel is not { IsProgressing: false } transferViewModel
                || ParentFolder?.Folder is not IModifiableFolder modifiableParent)
                return;

            try
            {
                using var cts = new CancellationTokenSource();
                var destination = await transferViewModel.SelectFolderAsync(TransferType.Move, cts);
                if (destination is null)
                    return;

                if (destination.Folder is not IModifiableFolder destinationFolder)
                    return;

                await transferViewModel.TransferAsync(innerChild, async (storable, token) =>
                {
                    // Move
                    var movedItem = await destinationFolder.MoveStorableFromAsync(storable, modifiableParent, false, token);
                    
                    // Remove existing from folder
                    ParentFolder.Items.RemoveMatch(x => x.Inner.Id == storable.Id);
                    
                    // Add to destination
                    destination.Items.Add(movedItem switch
                    {
                        IFile file => new FileViewModel(file, destination),
                        IFolder folder => new FolderViewModel(folder, destination, transferViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(movedItem))
                    });
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
        }
        
        [RelayCommand]
        protected virtual async Task CopyAsync(CancellationToken cancellationToken)
        {
            if (Inner is not IStorableChild innerChild)
                return;
            
            if (ParentFolder?.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            try
            {
                using var cts = new CancellationTokenSource();
                var destination = await transferViewModel.SelectFolderAsync(TransferType.Copy, cts);
                if (destination is null)
                    return;

                if (destination.Folder is not IModifiableFolder destinationFolder)
                    return;

                await transferViewModel.TransferAsync(innerChild, async (storable, token) =>
                {
                    // Copy
                    var copiedItem = await destinationFolder.CreateCopyOfStorableAsync(storable, false, token);
                    
                    // Add to destination
                    destination.Items.Add(copiedItem switch
                    {
                        IFile file => new FileViewModel(file, destination),
                        IFolder folder => new FolderViewModel(folder, destination, transferViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(copiedItem))
                    });
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
        }

        [RelayCommand]
        protected virtual async Task DeleteAsync(CancellationToken cancellationToken)
        {
            if (ParentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            // TODO: Show an overlay to ask the user. Deletion is always permanent
            await modifiableFolder.DeleteAsync((IStorableChild)Inner, cancellationToken);
            ParentFolder.Items.Remove(this);
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
