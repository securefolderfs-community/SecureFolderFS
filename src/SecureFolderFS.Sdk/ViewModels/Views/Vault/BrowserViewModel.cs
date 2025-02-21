using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public partial class BrowserViewModel : BaseDesignationViewModel
    {
        [ObservableProperty] private bool _IsSelecting;
        [ObservableProperty] private VaultViewModel _VaultViewModel;
        [ObservableProperty] private FolderViewModel? _CurrentFolder;
        [ObservableProperty] private TransferViewModel? _TransferViewModel;
        [ObservableProperty] private ObservableCollection<BreadcrumbItemViewModel> _Breadcrumbs;
        
        public IFolder BaseFolder { get; }
        
        public INavigator Navigator { get; }

        public BrowserViewModel(INavigator navigator, IFolder baseFolder, VaultViewModel vaultViewModel)
        {
            ServiceProvider = DI.Default;
            Navigator = navigator;
            BaseFolder = baseFolder;
            VaultViewModel = vaultViewModel;
            Breadcrumbs = new()
            {
                new(vaultViewModel.VaultName, NavigateBreadcrumbCommand)
            };
        }

        partial void OnCurrentFolderChanged(FolderViewModel? oldValue, FolderViewModel? newValue)
        {
            oldValue?.Items.UnselectAll();
            IsSelecting = false;
            Title = newValue?.Title;
            if (string.IsNullOrEmpty(Title))
                Title = VaultViewModel.VaultName;
        }

        [RelayCommand]
        protected virtual async Task NavigateBreadcrumbAsync(BreadcrumbItemViewModel? itemViewModel, CancellationToken cancellationToken)
        {
            if (itemViewModel is null)
                return;

            var lastIndex = Breadcrumbs.Count - 1;
            var breadcrumbIndex = Breadcrumbs.IndexOf(itemViewModel);
            var difference = lastIndex - breadcrumbIndex;
            for (var i = 0; i < difference; i++)
            {
                await Navigator.GoBackAsync();
            }
        }

        [RelayCommand]
        protected virtual async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (CurrentFolder is not null)
                await CurrentFolder.ListContentsAsync(cancellationToken);
        }
        
        [RelayCommand]
        protected virtual void ToggleSelection(bool? value = null)
        {
            IsSelecting = value ?? !IsSelecting;
            CurrentFolder?.Items.UnselectAll();
        }

        [RelayCommand]
        protected virtual async Task NewItemAsync(string? itemType, CancellationToken cancellationToken)
        {
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            IResult result;
            if (itemType is not ("Folder" or "File"))
            {
                var storableTypeViewModel = new StorableTypeOverlayViewModel();
                result = await OverlayService.ShowAsync(storableTypeViewModel);
                if (result.Aborted())
                    return;

                itemType = storableTypeViewModel.StorableType.ToString();
            }
            
            var newItemViewModel = new NewItemOverlayViewModel();
            result = await OverlayService.ShowAsync(newItemViewModel);
            if (result.Aborted() || newItemViewModel.ItemName is null)
                return;

            switch (itemType)
            {
                case "File":
                {
                    var file = await modifiableFolder.CreateFileAsync(newItemViewModel.ItemName, false, cancellationToken);
                    CurrentFolder.Items.Add(new FileViewModel(file, CurrentFolder));
                    break;
                }

                case "Folder":
                {
                    var folder = await modifiableFolder.CreateFolderAsync(newItemViewModel.ItemName, false, cancellationToken);
                    CurrentFolder.Items.Add(new FolderViewModel(folder, this, CurrentFolder));
                    break;
                }
            }
        }

        [RelayCommand]
        protected virtual async Task ImportItemAsync(string? itemType, CancellationToken cancellationToken)
        {
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            if (itemType is not ("Folder" or "File"))
            {
                var storableTypeViewModel = new StorableTypeOverlayViewModel();
                var result = await OverlayService.ShowAsync(storableTypeViewModel);
                if (result.Aborted())
                    return;

                itemType = storableTypeViewModel.StorableType.ToString();
            }

            switch (itemType)
            {
                case "File":
                {
                    var file = await FileExplorerService.PickFileAsync(null, false, cancellationToken);
                    if (file is null)
                        return;

                    var copiedFile = await modifiableFolder.CreateCopyOfAsync(file, false, cancellationToken);
                    CurrentFolder.Items.Add(new FileViewModel(copiedFile, CurrentFolder));
                    break;
                }

                case "Folder":
                {
                    var folder = await FileExplorerService.PickFolderAsync(false, cancellationToken);
                    if (folder is null)
                        return;
                    
                    var copiedFolder = await modifiableFolder.CreateCopyOfAsync(folder, false, cancellationToken);
                    CurrentFolder.Items.Add(new FolderViewModel(copiedFolder, this, CurrentFolder));
                    break;
                }
            }
        }
    }
}
