using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Inject<IFileExplorerService>]
    [Bindable(true)]
    public abstract partial class BrowserItemViewModel : ObservableObject, IWrapper<IStorable>, IViewable, IAsyncInitialize
    {
        protected readonly TransferViewModel transferViewModel;
        
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private IImage? _Thumbnail;

        /// <inheritdoc/>
        public abstract IStorable Inner { get; }

        /// <summary>
        /// Gets the parent <see cref="FolderViewModel"/> that this item resides in, if any.
        /// </summary>
        public FolderViewModel? ParentFolder { get; }

        protected BrowserItemViewModel(TransferViewModel transferViewModel, FolderViewModel? parentFolder)
        {
            ServiceProvider = DI.Default;
            ParentFolder = parentFolder;
            this.transferViewModel = transferViewModel;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        [RelayCommand]
        protected virtual async Task MoveAsync(CancellationToken cancellationToken)
        {
            transferViewModel.IsTransferring = true;
            transferViewModel.TranferredItems.Add(this);
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
            
            var destinationFolder = await FileExplorerService.PickFolderAsync(false, cancellationToken);
            if (destinationFolder is not IModifiableFolder modifiableFolder)
                return;
            
            switch (Inner)
            {
                case IFile file:
                    await modifiableFolder.CreateCopyOfAsync(file, false, cancellationToken);
                    break;
                
                case IFolder folder:
                    await modifiableFolder.CreateCopyOfAsync(folder, false, cancellationToken);
                    break;
                
                default: return;
            }

            await parentModifiableFolder.DeleteAsync((IStorableChild)Inner, cancellationToken);
            ParentFolder.Items.Remove(this);
        }
        
        [RelayCommand]
        protected abstract Task OpenAsync(CancellationToken cancellationToken);
    }
}
