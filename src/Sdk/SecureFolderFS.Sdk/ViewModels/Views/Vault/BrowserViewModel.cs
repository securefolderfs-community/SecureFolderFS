using System;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Shared.Helpers;

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

        public LayoutsViewModel Layouts { get; }

        public INavigator InnerNavigator { get; }

        public INavigator? OuterNavigator { get; }

        [Obsolete("Use FileSystemOptions instead.")]
        public IVFSRoot? StorageRoot { get; init; }

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

            CurrentFolder?.Dispose();
            base.OnDisappearing();
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            if (OuterNavigator is null || TransferViewModel is null)
                return null;

            try
            {
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
                await OuterNavigator.GoBackAsync();
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
        protected virtual async Task ChangeViewOptionsAsync(CancellationToken cancellationToken)
        {
            if (CurrentFolder is null)
                return;

            var originalSortOption = Layouts.CurrentSortOption;
            await OverlayService.ShowAsync(Layouts);

            if (originalSortOption != Layouts.CurrentSortOption)
                Layouts.GetSorter()?.SortCollection(CurrentFolder.Items, CurrentFolder.Items);
        }

        [RelayCommand]
        protected virtual async Task NewItemAsync(string? itemType, CancellationToken cancellationToken)
        {
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

                    itemType = storableTypeViewModel.StorableType.ToString();
                }
            }

            var newItemViewModel = new NewItemOverlayViewModel();
            result = await OverlayService.ShowAsync(newItemViewModel);
            if (result.Aborted() || newItemViewModel.ItemName is null)
                return;

            var formattedName = CollisionHelpers.GetAvailableName(
                    FormattingHelpers.SanitizeItemName(newItemViewModel.ItemName, "New item"),
                    CurrentFolder.Items.Select(x => x.Inner.Name));
            switch (itemType)
            {
                case "File":
                {
                    var file = await modifiableFolder.CreateFileAsync(formattedName, false, cancellationToken);
                    CurrentFolder.Items.Insert(new FileViewModel(file, this, CurrentFolder), Layouts.GetSorter());
                    break;
                }

                case "Folder":
                {
                    var folder = await modifiableFolder.CreateFolderAsync(formattedName, false, cancellationToken);
                    CurrentFolder.Items.Insert(new FolderViewModel(folder, this, CurrentFolder), Layouts.GetSorter());
                    break;
                }
            }
        }

        [RelayCommand]
        protected virtual async Task ImportItemAsync(string? itemType, CancellationToken cancellationToken)
        {
            if (CurrentFolder?.Folder is not IModifiableFolder modifiableFolder)
                return;

            if (TransferViewModel is null)
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

                    TransferViewModel.TransferType = TransferType.Copy;
                    using var cts = TransferViewModel.GetCancellation(cancellationToken);
                    await TransferViewModel.TransferAsync([ file ], async (item, token) =>
                    {
                        var copiedFile = await modifiableFolder.CreateCopyOfAsync(item, false, token);
                        CurrentFolder.Items.Insert(new FileViewModel(copiedFile, this, CurrentFolder), Layouts.GetSorter());
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
                        var copiedFolder = await modifiableFolder.CreateCopyOfAsync(item, false, reporter, token);
                        CurrentFolder.Items.Insert(new FolderViewModel(copiedFolder, this, CurrentFolder), Layouts.GetSorter());
                    }, cts.Token);

                    break;
                }
            }
        }
    }
}
