using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.AppModels.Sorters;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Storage.Scanners;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public partial class BrowserViewModel : BaseDesignationViewModel, IFolderPicker, IDisposable
    {
        private readonly IViewable? _rootView;

        [ObservableProperty] private bool _IsSelecting;
        [ObservableProperty] private FolderViewModel? _CurrentFolder;
        [ObservableProperty] private TransferViewModel? _TransferViewModel;
        [ObservableProperty] private ObservableCollection<BreadcrumbItemViewModel> _Breadcrumbs;

        public IFolder BaseFolder { get; }

        public LayoutsViewModel Layouts { get; }

        public INavigator InnerNavigator { get; }

        public INavigator? OuterNavigator { get; }

        /// <summary>
        /// Gets a value indicating whether the folder selection process is currently active.
        /// </summary>
        public bool IsPickingFolder { get; protected set; }

        /// <summary>
        /// Gets the thumbnail cache for this browser instance.
        /// </summary>
        public ThumbnailCacheModel ThumbnailCache { get; }

        [Obsolete("Use FileSystemOptions instead.")]
        public IVfsRoot? StorageRoot { get; init; }

        public FileSystemOptions Options { get; }

        public BrowserViewModel(IFolder baseFolder, FileSystemOptions options, INavigator innerNavigator, INavigator? outerNavigator, IViewable? rootView)
        {
            ServiceProvider = DI.Default;
            _rootView = rootView;
            Layouts = new();
            InnerNavigator = innerNavigator;
            OuterNavigator = outerNavigator;
            BaseFolder = baseFolder;
            Options = options;
            ThumbnailCache = new();
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
            if (TransferViewModel is not null)
            {
                if (TransferViewModel.IsVisible && !TransferViewModel.IsProgressing)
                    TransferViewModel?.CancelCommand.Execute(null);
            }

            // Do not dispose CurrentFolder here because the page may reappear (e.g., after an
            // overlay closes) and its items are still bound. Disposal happens in Dispose()
            base.OnDisappearing();
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            if (OuterNavigator is null || TransferViewModel is null)
                return null;

            try
            {
                IsPickingFolder = true;
                TransferViewModel.IsPickingFolder = true;
                await OuterNavigator.NavigateAsync(this);
                using var cts = TransferViewModel.GetCancellation(cancellationToken);
                return await TransferViewModel.PickFolderAsync(new TransferOptions(TransferType.Select), false, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            finally
            {
                IsPickingFolder = false;
                TransferViewModel.IsPickingFolder = false;
                await OuterNavigator.GoBackAsync();
                Dispose();
            }
        }

        partial void OnCurrentFolderChanged(FolderViewModel? oldValue, FolderViewModel? newValue)
        {
            oldValue?.Items.UnselectAll();
            IsSelecting = false;
            Title = newValue?.Title;
            if (string.IsNullOrEmpty(Title))
                Title = _rootView?.Title;
        }

        partial void OnIsSelectingChanged(bool oldValue, bool newValue)
        {
            if (oldValue && !newValue)
                CurrentFolder?.Items.UnselectAll();
        }

        [RelayCommand]
        protected virtual async Task NavigateBreadcrumbAsync(BreadcrumbItemViewModel? itemViewModel, CancellationToken cancellationToken)
        {
            if (itemViewModel is null || itemViewModel == Breadcrumbs.LastOrDefault())
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
        protected virtual void SelectAll()
        {
            if (!IsSelecting || CurrentFolder is null)
                return;

            CurrentFolder.Items.SelectAll();
        }

        [RelayCommand]
        protected virtual Task MoveSelectedAsync(CancellationToken cancellationToken)
        {
            return ExecuteOnSelectionAsync(static item => item.MoveCommand.ExecuteAsync(null));
        }

        [RelayCommand]
        protected virtual Task CopySelectedAsync(CancellationToken cancellationToken)
        {
            return ExecuteOnSelectionAsync(static item => item.CopyCommand.ExecuteAsync(null));
        }

        [RelayCommand]
        protected virtual Task ExportSelectedAsync(CancellationToken cancellationToken)
        {
            return ExecuteOnSelectionAsync(static item => item.ExportCommand.ExecuteAsync(null));
        }

        [RelayCommand]
        protected virtual Task DeleteSelectedAsync(CancellationToken cancellationToken)
        {
            return ExecuteOnSelectionAsync(static item => item.DeleteCommand.ExecuteAsync(null));
        }

        private async Task ExecuteOnSelectionAsync(Func<BrowserItemViewModel, Task> action)
        {
            // The item-level commands already operate on the whole selection
            // when IsSelecting is active - delegate to any selected item
            var selectedItem = CurrentFolder?.SelectedItems.FirstOrDefault();
            if (selectedItem is null)
                return;

            await action(selectedItem);
        }

        [RelayCommand]
        protected virtual async Task SearchAsync()
        {
            if (CurrentFolder?.Inner is not IFolder searchedFolder)
                return;

            var searchModel = new BrowserFolderSearchModel(
                new ShallowFolderScanner(searchedFolder),
                new DeepFolderScanner(searchedFolder));

            var overlayViewModel = new BrowserSearchOverlayViewModel(
                searchedFolder,
                searchModel,
                ThumbnailCache,
                NavigateToSearchResultAsync).WithInitAsync();

            await OverlayService.ShowAsync(overlayViewModel);
            overlayViewModel.Dispose();
        }

        private async Task NavigateToSearchResultAsync(SearchBrowserItemViewModel searchItemViewModel, CancellationToken cancellationToken)
        {
            if (CurrentFolder is null)
                return;

            var destinationFolder = searchItemViewModel.Inner switch
            {
                IFolder folder => folder,
                IStorableChild child => await child.GetParentAsync(cancellationToken),
                _ => null
            };

            if (destinationFolder is null)
                return;

            if (CurrentFolder.Folder.Id == destinationFolder.Id)
                return;

            var navigationChain = new Stack<IFolder>();
            var currentFolder = destinationFolder;
            while (CurrentFolder.Folder.Id != currentFolder.Id)
            {
                navigationChain.Push(currentFolder);

                if (currentFolder is not IStorableChild childFolder)
                    return;

                var parentFolder = await childFolder.GetParentAsync(cancellationToken);
                if (parentFolder is null)
                    return;

                currentFolder = parentFolder;
            }

            var parentFolderViewModel = CurrentFolder;
            while (navigationChain.TryPop(out var folder))
            {
                var targetViewModel = new FolderViewModel(folder, this, parentFolderViewModel);
                if (targetViewModel.Items.IsEmpty())
                    _ = targetViewModel.ListContentsAsync(cancellationToken);

                await InnerNavigator.NavigateAsync(targetViewModel);
                parentFolderViewModel = targetViewModel;
            }
        }

        [RelayCommand]
        protected virtual async Task ChangeViewOptionsAsync(CancellationToken cancellationToken)
        {
            if (CurrentFolder is null)
                return;

            var originalSortOption = Layouts.CurrentSortOption;
            var originalIsAscending = Layouts.IsAscending;
            await OverlayService.ShowAsync(Layouts);

            if (originalSortOption != Layouts.CurrentSortOption || originalIsAscending != Layouts.IsAscending)
            {
                // Date sorting needs modification dates, which are loaded lazily and may be
                // missing for items that were never scrolled into view - relist to fetch them
                if (Layouts.GetSorter() is DateSorter && CurrentFolder.Items.Any(x => x.LastModified is null))
                    await CurrentFolder.ListContentsAsync(cancellationToken);
                else
                    Layouts.GetSorter()?.SortCollection(CurrentFolder.Items, CurrentFolder.Items);
            }
        }

        [RelayCommand]
        protected virtual async Task NewItemAsync(string? itemType, CancellationToken cancellationToken)
        {
            if (Options.IsReadOnly)
                return;

            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            IResult result;
            if (itemType is not ("Folder" or "File"))
            {
                if (TransferViewModel?.IsPickingFolder ?? false)
                    itemType = nameof(StorableType.Folder);
                else
                {
                    var storableTypeViewModel = new StorableTypeOverlayViewModel();
                    result = await OverlayService.ShowAsync(storableTypeViewModel);
                    if (result.Aborted())
                        return;

                    itemType = storableTypeViewModel.SelectedOption ?? storableTypeViewModel.StorableType.ToString();
                }
            }

            var newItemViewModel = new NewItemOverlayViewModel();
            result = await OverlayService.ShowAsync(newItemViewModel);
            if (result.Aborted() || newItemViewModel.ItemName is null)
                return;

            // The collision check below runs against the loaded items, so make sure
            // the listing is complete - otherwise CreateFileAsync(overwrite: false)
            // could silently return an existing item instead of creating a new one
            if (CurrentFolder.Items.IsEmpty())
                await CurrentFolder.ListContentsAsync(cancellationToken);

            var formattedName = CollisionHelpers.GetAvailableName(
                    FormattingHelpers.SanitizeItemName(newItemViewModel.ItemName, "New item"),
                    CurrentFolder.Items.Select(x => x.Inner.Name));
            try
            {
                switch (itemType)
                {
                    case "File":
                    {
                        var file = await modifiableFolder.CreateFileAsync(formattedName, false, cancellationToken);
                        CurrentFolder.Items.Insert(new FileViewModel(file, this, CurrentFolder).WithInitAsync(), Layouts.GetSorter());
                        break;
                    }

                    case "Folder":
                    {
                        var folder = await modifiableFolder.CreateFolderAsync(formattedName, false, cancellationToken);
                        CurrentFolder.Items.Insert(new FolderViewModel(folder, this, CurrentFolder).WithInitAsync(), Layouts.GetSorter());
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation, nothing to report
            }
            catch (Exception ex)
            {
                if (TransferViewModel is not null)
                    await TransferViewModel.ReportErrorAsync($"{"OperationFailed".ToLocalized()} ({ex.Message})");
            }
        }

        [RelayCommand]
        protected virtual async Task ImportItemAsync(string? itemType, CancellationToken cancellationToken)
        {
            if (Options.IsReadOnly)
                return;

            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            if (TransferViewModel is null)
                return;

            if (itemType is not ("Folder" or "File"))
            {
                var storableTypeViewModel = new StorableTypeOverlayViewModel
                {
                    IncludeGallery = true
                };
                var result = await OverlayService.ShowAsync(storableTypeViewModel);
                if (result.Aborted())
                    return;

                itemType = storableTypeViewModel.SelectedOption ?? storableTypeViewModel.StorableType.ToString();
            }

            try
            {
                switch (itemType)
                {
                    case "File":
                    {
                        var file = await FileExplorerService.PickFileAsync(null, false, cancellationToken);
                        if (file is null)
                            return;

                        TransferViewModel.TransferType = TransferType.Copy;
                        using var cts = TransferViewModel.GetCancellation(cancellationToken);
                        await TransferViewModel.TransferAsync([ file ], async (item, token) =>
                        {
                            // Get available name to avoid collision
                            var availableName = CollisionHelpers.GetAvailableName(item.Name, CurrentFolder.Items.Select(x => x.Inner.Name));

                            // Copy
                            var copiedFile = await modifiableFolder.CreateCopyOfAsync(item, false, availableName, token);

                            // Add to destination
                            CurrentFolder.Items.Insert(new FileViewModel(copiedFile, this, CurrentFolder).WithInitAsync(), Layouts.GetSorter());
                        }, cts.Token);

                        break;
                    }

                    case "Folder":
                    {
                        var folder = await FileExplorerService.PickFolderAsync(null, false, cancellationToken);
                        if (folder is null)
                            return;

                        TransferViewModel.TransferType = TransferType.Copy;
                        using var cts = TransferViewModel.GetCancellation(cancellationToken);
                        await TransferViewModel.TransferAsync([ folder ], async (item, reporter, token) =>
                        {
                            // Get available name to avoid collision
                            var availableName = CollisionHelpers.GetAvailableName(item.Name, CurrentFolder.Items.Select(x => x.Inner.Name));

                            // Copy
                            var copiedFolder = await modifiableFolder.CreateCopyOfAsync(item, false, availableName, reporter, token);

                            // Add to destination
                            CurrentFolder.Items.Insert(new FolderViewModel(copiedFolder, this, CurrentFolder).WithInitAsync(), Layouts.GetSorter());
                        }, cts.Token);

                        break;
                    }

                    case var t when t == "Gallery".ToLocalized():
                    {
                        var galleryItems = (await FileExplorerService.PickGalleryItemsAsync(cancellationToken)).OfType<IFile>().ToArray();
                        if (galleryItems.Length == 0)
                            return;

                        TransferViewModel.TransferType = TransferType.Copy;
                        using var cts = TransferViewModel.GetCancellation(cancellationToken);
                        var existingNames = new HashSet<string>(CurrentFolder.Items.Select(x => x.Inner.Name), StringComparer.OrdinalIgnoreCase);
                        await TransferViewModel.TransferAsync(galleryItems, async (item, token) =>
                        {
                            // Get available name to avoid collision
                            var availableName = CollisionHelpers.GetAvailableName(item.Name, existingNames);

                            // Copy
                            var copiedFile = await modifiableFolder.CreateCopyOfAsync(item, false, availableName, token);
                            existingNames.Add(availableName);

                            // Add to destination
                            CurrentFolder.Items.Insert(new FileViewModel(copiedFile, this, CurrentFolder).WithInitAsync(), Layouts.GetSorter());
                        }, cts.Token);

                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation, nothing to report
            }
            catch (Exception ex)
            {
                await TransferViewModel.ReportErrorAsync($"{"OperationFailed".ToLocalized()} ({ex.Message})");
            }
            finally
            {
                await TransferViewModel.HideAsync();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ThumbnailCache.Dispose();

            // Dispose every folder accumulated in the navigation stack
            if (InnerNavigator is INavigationService navigationService)
            {
                foreach (var view in navigationService.Views)
                {
                    if (view is not FolderViewModel folderViewModel || folderViewModel == CurrentFolder)
                        continue;

                    folderViewModel.Items.DisposeAll();
                    folderViewModel.Items.Clear();
                    folderViewModel.Dispose();
                }
            }

            CurrentFolder?.Dispose();
            CurrentFolder?.Items.DisposeAll();
            CurrentFolder?.Items.Clear();
        }
    }
}
