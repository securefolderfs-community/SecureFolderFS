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
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public partial class BrowserViewModel : BaseDesignationViewModel, IFolderPicker
    {
        private readonly IViewable? _rootView;

        [ObservableProperty] private bool _IsSelecting;
        [ObservableProperty] private FolderViewModel? _CurrentFolder;
        [ObservableProperty] private TransferViewModel? _TransferViewModel;
        [ObservableProperty] private ObservableCollection<BreadcrumbItemViewModel> _Breadcrumbs;

        public IFolder BaseFolder { get; }

        public INavigator InnerNavigator { get; }

        public INavigator? OuterNavigator { get; }

        public BrowserViewModel(IFolder baseFolder, INavigator innerNavigator, INavigator? outerNavigator, IViewable? rootView)
        {
            ServiceProvider = DI.Default;
            _rootView = rootView;
            InnerNavigator = innerNavigator;
            OuterNavigator = outerNavigator;
            BaseFolder = baseFolder;
            Breadcrumbs = [ new(rootView?.Title, NavigateBreadcrumbCommand) ];
        }

        partial void OnCurrentFolderChanged(FolderViewModel? oldValue, FolderViewModel? newValue)
        {
            oldValue?.Items.UnselectAll();
            IsSelecting = false;
            Title = newValue?.Title;
            if (string.IsNullOrEmpty(Title))
                Title = _rootView?.Title;
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
                await InnerNavigator.GoBackAsync();
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
                    CurrentFolder.InsertSorted(new FileViewModel(file, CurrentFolder));
                    break;
                }

                case "Folder":
                {
                    var folder = await modifiableFolder.CreateFolderAsync(newItemViewModel.ItemName, false, cancellationToken);
                    CurrentFolder.InsertSorted(new FolderViewModel(folder, this, CurrentFolder));
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
                    CurrentFolder.InsertSorted(new FileViewModel(copiedFile, CurrentFolder));
                    break;
                }

                case "Folder":
                {
                    var folder = await FileExplorerService.PickFolderAsync(null, false, cancellationToken);
                    if (folder is null)
                        return;
                    
                    var copiedFolder = await modifiableFolder.CreateCopyOfAsync(folder, false, cancellationToken);
                    CurrentFolder.InsertSorted(new FolderViewModel(copiedFolder, this, CurrentFolder));
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(FilterOptions? filter, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            if (OuterNavigator is null || TransferViewModel is null)
                return null;

            await OuterNavigator.NavigateAsync(this);
            var pickedFolder = await TransferViewModel.PickFolderAsync(null, false, cancellationToken);
            if (pickedFolder is null)
                return null;

            await OuterNavigator.GoBackAsync();
            return pickedFolder;
        }
    }
}
