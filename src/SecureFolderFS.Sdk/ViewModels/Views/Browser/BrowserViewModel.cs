using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Inject<IOverlayService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public partial class BrowserViewModel : ObservableObject, IViewDesignation
    {
        private readonly INavigator _navigator;
        
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private VaultViewModel _VaultViewModel;
        [ObservableProperty] private FolderViewModel? _CurrentFolder;
        [ObservableProperty] private ObservableCollection<BreadcrumbItemViewModel> _Breadcrumbs;
        
        public IFolder BaseFolder { get; }

        public BrowserViewModel(FolderViewModel folderViewModel, IFolder baseFolder, VaultViewModel vaultViewModel)
        {
            ServiceProvider = DI.Default;
            _navigator = folderViewModel.Navigator;
            BaseFolder = baseFolder;
            VaultViewModel = vaultViewModel;
            CurrentFolder = folderViewModel;
            Breadcrumbs = new()
            {
                new(vaultViewModel.VaultName, NavigateBreadcrumbCommand)
            };
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
        protected virtual async Task NavigateBreadcrumbAsync(BreadcrumbItemViewModel? itemViewModel, CancellationToken cancellationToken)
        {
            if (itemViewModel is null)
                return;

            var lastIndex = Breadcrumbs.Count - 1;
            var breadcrumbIndex = Breadcrumbs.IndexOf(itemViewModel);
            var difference = lastIndex - breadcrumbIndex;
            for (var i = 0; i < difference; i++)
            {
                await _navigator.GoBackAsync();
            }
        }

        [RelayCommand]
        protected virtual async Task NewItemAsync(string? itemType, CancellationToken cancellationToken)
        {
            if (itemType is null)
                return;

            itemType = itemType.ToLower();
            if (itemType is not ("folder" or "file"))
                return;
            
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;
            
            var viewModel = new NewItemOverlayViewModel();
            var result = await OverlayService.ShowAsync(viewModel);
            if (result.Aborted() || viewModel.ItemName is null)
                return;

            switch (itemType)
            {
                case "file":
                {
                    var file = await modifiableFolder.CreateFileAsync(viewModel.ItemName, false, cancellationToken);
                    CurrentFolder.Items.Add(new FileViewModel(file, CurrentFolder));
                    break;
                }

                case "folder":
                {
                    var folder = await modifiableFolder.CreateFolderAsync(viewModel.ItemName, false, cancellationToken);
                    CurrentFolder.Items.Add(new FolderViewModel(folder, CurrentFolder.Navigator, null, CurrentFolder));
                    break;
                }
            }
        }

        [RelayCommand]
        protected virtual async Task ImportItemAsync(string? itemType, CancellationToken cancellationToken)
        {
            if (itemType is null)
                return;

            itemType = itemType.ToLower();
            if (itemType is not ("folder" or "file"))
                return;
            
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            switch (itemType)
            {
                case "file":
                {
                    var file = await FileExplorerService.PickFileAsync(null, false, cancellationToken);
                    if (file is null)
                        return;

                    var copiedFile = await modifiableFolder.CreateCopyOfAsync(file, false, cancellationToken);
                    CurrentFolder.Items.Add(new FileViewModel(copiedFile, CurrentFolder));
                    break;
                }

                case "folder":
                {
                    var folder = await FileExplorerService.PickFolderAsync(false, cancellationToken);
                    if (folder is null)
                        return;
                    
                    var copiedFolder = await modifiableFolder.CreateCopyOfAsync(folder, false, cancellationToken);
                    CurrentFolder.Items.Add(new FolderViewModel(copiedFolder, CurrentFolder.Navigator, null, CurrentFolder));
                    break;
                }
            }
        }
    }
}
