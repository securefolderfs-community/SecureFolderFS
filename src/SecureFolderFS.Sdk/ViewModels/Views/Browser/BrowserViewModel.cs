using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Inject<IOverlayService>]
    [Bindable(true)]
    public partial class BrowserViewModel : ObservableObject, IViewDesignation, INavigatable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private VaultViewModel _VaultViewModel;
        [ObservableProperty] private FolderViewModel? _CurrentFolder;
        
        public IFolder BaseFolder { get; }
        
        /// <inheritdoc/>
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public BrowserViewModel(FolderViewModel folderViewModel, IFolder baseFolder, VaultViewModel vaultViewModel)
        {
            ServiceProvider = DI.Default;
            BaseFolder = baseFolder;
            VaultViewModel = vaultViewModel;
            CurrentFolder = folderViewModel;
        }
        
        /// <inheritdoc/>
        public virtual void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public virtual void OnDisappearing()
        {
        }

        partial void OnCurrentFolderChanged(FolderViewModel? value)
        {
            Title = value?.Title;
            if (string.IsNullOrEmpty(Title))
                Title = VaultViewModel.VaultName;
        }

        [RelayCommand]
        protected virtual async Task NewFolderAsync(CancellationToken cancellationToken)
        {
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            // TODO: Add NewFolderOverlayViewModel
            _ = modifiableFolder;
            var result = await OverlayService.ShowAsync(null!);
            if (result is IResult<IFolder> { Successful: true, Value: not null } folderResult)
                CurrentFolder.Items.Add(new FolderViewModel(folderResult.Value, CurrentFolder.Navigator));
        }

        [RelayCommand]
        protected virtual async Task CopyAsync(IEnumerable items, CancellationToken cancellationToken)
        {
            // TODO: Add CopyOverlayViewModel
            var result = await OverlayService.ShowAsync(null!);
            if (result is not IResult<IFolder> { Successful: true, Value: ICreateCopyOf folderCreateCopyOf /*IDirectCopy directCopy*/ })
                return;

            foreach (IStorableChild item in items)
            {
                // TODO(ns)
                //folderCreateCopyOf.CreateCopyOfAsync(item, default, cancellationToken);
            }
        }

        [RelayCommand]
        protected virtual async Task MoveAsync(IEnumerable items, CancellationToken cancellationToken)
        {
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            // TODO: Add MoveOverlayViewModel
            var result = await OverlayService.ShowAsync(null!);
            if (result is not IResult<IFolder> { Successful: true, Value: IMoveFrom folderMoveFrom })
                return;

            foreach (IStorableChild item in items)
            {
                // TODO(ns)
                //await folderMoveFrom.MoveFromAsync(item, modifiableFolder, default, cancellationToken);
                //folderViewModel.Items.RemoveMatch(x => x.Inner.Id == item.Id);
            }
        }

        [RelayCommand]
        protected virtual async Task DeleteAsync(IEnumerable items, CancellationToken cancellationToken)
        {
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;
            
            // TODO: Add DeletionOverlayViewModel
            var result = await OverlayService.ShowAsync(null!);
            if (!result.Successful)
                return;

            foreach (IStorableChild item in items)
            {
                await modifiableFolder.DeleteAsync(item, cancellationToken);
                CurrentFolder.Items.RemoveMatch(x => x.Inner.Id == item.Id);
            }
        }
    }
}
