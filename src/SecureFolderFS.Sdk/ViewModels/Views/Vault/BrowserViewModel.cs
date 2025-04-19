using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.VirtualFileSystem;

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

        public ViewOptionsViewModel ViewOptions { get; }

        public required IVFSRoot StorageRoot { get; init; }

        public BrowserViewModel(IFolder baseFolder, INavigator innerNavigator, INavigator? outerNavigator, IViewable? rootView)
        {
            ServiceProvider = DI.Default;
            _rootView = rootView;
            ViewOptions = new();
            InnerNavigator = innerNavigator;
            OuterNavigator = outerNavigator;
            BaseFolder = baseFolder;
            Breadcrumbs = [ new(rootView?.Title, NavigateBreadcrumbCommand) ];
        }

        /// <inheritdoc/>
        public override void OnAppearing()
        {
            _ = CurrentFolder?.ListContentsAsync();
            base.OnAppearing();
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            CurrentFolder?.Dispose();
            base.OnDisappearing();
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(FilterOptions? filter, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            if (OuterNavigator is null || TransferViewModel is null)
                return null;

            await OuterNavigator.NavigateAsync(this);
            var cts = TransferViewModel.GetCancellation();
            var pickedFolder = await TransferViewModel.PickFolderAsync(new TransferFilter(TransferType.Select), false, cts.Token);
            if (pickedFolder is null)
                return null;

            await OuterNavigator.GoBackAsync();
            return pickedFolder;
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
        protected virtual async Task ChangeViewOptionsAsync(CancellationToken cancellationToken)
        {
            if (CurrentFolder is null)
                return;

            var originalSortOption = ViewOptions.CurrentSortOption;
            await OverlayService.ShowAsync(ViewOptions);

            if (originalSortOption != ViewOptions.CurrentSortOption)
                ViewOptions.GetSorter()?.SortCollection(CurrentFolder.Items, CurrentFolder.Items);
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
                    CurrentFolder.Items.Insert(new FileViewModel(file, this, CurrentFolder), ViewOptions.GetSorter());
                    break;
                }

                case "Folder":
                {
                    var folder = await modifiableFolder.CreateFolderAsync(newItemViewModel.ItemName, false, cancellationToken);
                    CurrentFolder.Items.Insert(new FolderViewModel(folder, this, CurrentFolder), ViewOptions.GetSorter());
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
                    CurrentFolder.Items.Insert(new FileViewModel(copiedFile, this, CurrentFolder), ViewOptions.GetSorter());

                    break;
                }

                case "Folder":
                {
                    if (TransferViewModel is null)
                        return;

                    var folder = await FileExplorerService.PickFolderAsync(null, false, cancellationToken);
                    if (folder is null)
                        return;

                    await TransferViewModel.TransferAsync(folder, async (item, token) =>
                    {
                        var copiedFolder = await modifiableFolder.CreateCopyOfAsync(item, false, token);
                        CurrentFolder.Items.Insert(new FolderViewModel(copiedFolder, this, CurrentFolder), ViewOptions.GetSorter());
                    }, cancellationToken);

                    break;
                }
            }
        }
    }
}
