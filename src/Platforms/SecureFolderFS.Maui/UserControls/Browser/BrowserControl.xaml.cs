using System.Windows.Input;
using APES.UI.XF;
using OwlCore.Storage;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;

#if IOS || MACCATALYST
using SecureFolderFS.Maui.Platforms.iOS.Storage;
#endif

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl : ContentView
    {
        private readonly DeferredInitialization<IFolder> _deferredInitialization;
        private readonly ISettingsService _settingsService;

        public BrowserControl()
        {
            _deferredInitialization = new(UI.Constants.Browser.THUMBNAIL_MAX_PARALLELISATION);
            _settingsService = DI.Service<ISettingsService>();
            InitializeComponent();
        }

        private void RefreshView_Refreshing(object? sender, EventArgs e)
        {
            if (sender is not RefreshView refreshView)
                return;

            RefreshCommand?.Execute(null);
            refreshView.IsRefreshing = false;
        }

        private async void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not View { BindingContext: BrowserItemViewModel itemViewModel } view)
                return;

            if (IsSelecting)
                itemViewModel.IsSelected = !itemViewModel.IsSelected;
            else
            {
                view.IsEnabled = false;
                await itemViewModel.OpenCommand.ExecuteAsync(null);
                view.IsEnabled = true;
            }
        }

        private void ItemContainer_Loaded(object? sender, EventArgs e)
        {
            if (!_settingsService.UserSettings.AreThumbnailsEnabled)
                return;

            if (sender is not BindableObject { BindingContext: FileViewModel fileViewModel })
                return;

            if (fileViewModel.Thumbnail is not null)
                return;

            _deferredInitialization.SetContext(fileViewModel.ParentFolder!.Folder);
            _deferredInitialization.Enqueue(fileViewModel);
        }

        private void DragGestureRecognizer_DragStarting(object? sender, DragStartingEventArgs e)
        {
            if (sender is not ContextMenuContainer { BindingContext: BrowserItemViewModel itemViewModel })
                return;

            e.Data.Properties["DraggedItem"] = itemViewModel;
        }

        private void DropGestureRecognizer_DragOver(object? sender, DragEventArgs e)
        {
            if (sender is not DropGestureRecognizer { Parent: View view })
                return;

            if (view.BindingContext is not FolderViewModel)
            {
                e.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            e.AcceptedOperation = DataPackageOperation.Copy;
            view.BackgroundColor = Color.FromArgb("#30808080");
        }

        private void DropGestureRecognizer_DragLeave(object? sender, DragEventArgs e)
        {
            if (sender is not DropGestureRecognizer { Parent: View view })
                return;

            view.BackgroundColor = Colors.Transparent;
        }

        private async void DropGestureRecognizer_Drop(object? sender, DropEventArgs e)
        {
            if (sender is not ContextMenuContainer { BindingContext: FolderViewModel folderViewModel })
                return;

            // Handle internal drag-and-drop (from within the app)
            if (e.Data.Properties.TryGetValue("DraggedItem", out var draggedItemObj) && draggedItemObj is BrowserItemViewModel draggedItem)
            {
                // Disallow dropping on itself
                if (draggedItem == folderViewModel)
                    return;

                // Disallow dropping a folder into its own subfolder
                if (folderViewModel.Inner.Id.Contains(draggedItem.Inner.Id, StringComparison.InvariantCultureIgnoreCase))
                    return;

                await MoveItemToFolderAsync(draggedItem, folderViewModel);
                return;
            }

            // Handle external files dropped from system apps (e.g., Files app)
            await CopyExternalFilesToFolderAsync(e, folderViewModel);
        }

        private void CollectionDropGestureRecognizer_DragOver(object? sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void CollectionDropGestureRecognizer_DragLeave(object? sender, DragEventArgs e)
        {
            // No visual feedback needed for collection drop
        }

        private async void CollectionDropGestureRecognizer_Drop(object? sender, DropEventArgs e)
        {
            // Handle internal drag-and-drop (from within the app)
            if (e.Data.Properties.TryGetValue("DraggedItem", out var draggedItemObj) && draggedItemObj is BrowserItemViewModel draggedItem)
            {
                if (draggedItem.ParentFolder is null)
                    return;

                // Get the current folder from the BrowserViewModel
                var browserViewModel = draggedItem.BrowserViewModel;
                var currentFolder = browserViewModel.CurrentFolder;
                
                if (currentFolder is null)
                    return;

                // Don't move if the item is already in the current folder
                if (draggedItem.ParentFolder == currentFolder)
                    return;

                // Disallow dropping a folder into its own subfolder
                if (currentFolder.Inner.Id.Contains(draggedItem.Inner.Id, StringComparison.InvariantCultureIgnoreCase))
                    return;

                await MoveItemToFolderAsync(draggedItem, currentFolder);
                return;
            }

            // Handle external files dropped from system apps (e.g., Files app)
            // Get the current folder from the ItemsSource binding
            if (ItemsSource is not { Count: >= 0 } items)
                return;

            // Try to get the BrowserViewModel from the first item, or from the binding context
            var firstItem = items.FirstOrDefault();
            var targetBrowserViewModel = firstItem?.BrowserViewModel;
            var targetFolder = targetBrowserViewModel?.CurrentFolder;

            if (targetFolder is null)
                return;

            await CopyExternalFilesToFolderAsync(e, targetFolder);
        }

        /// <summary>
        /// Moves a dragged item to the specified destination folder.
        /// </summary>
        /// <param name="draggedItem">The item being dragged.</param>
        /// <param name="destinationFolderViewModel">The destination folder view model.</param>
        private static async Task MoveItemToFolderAsync(BrowserItemViewModel draggedItem, FolderViewModel destinationFolderViewModel)
        {
            if (draggedItem.ParentFolder?.Folder is not IModifiableFolder sourceFolder)
                return;

            if (destinationFolderViewModel.Folder is not IModifiableFolder destinationFolder)
                return;

            if (draggedItem.Inner is not IStorableChild itemToMove)
                return;

            var browserViewModel = draggedItem.BrowserViewModel;
            if (browserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            try
            {
                transferViewModel.TransferType = TransferType.Move;
                using var cts = transferViewModel.GetCancellation();

                await transferViewModel.TransferAsync([ itemToMove ], async (item, reporter, token) =>
                {
                    // Move
                    var movedItem = await destinationFolder.MoveStorableFromAsync(item, sourceFolder, false, reporter, token);

                    // Remove existing from source folder
                    draggedItem.ParentFolder.Items.RemoveMatch(x => x.Inner.Id == item.Id)?.Dispose();

                    // Add to destination
                    destinationFolderViewModel.Items.Insert(movedItem switch
                    {
                        IFile file => new FileViewModel(file, browserViewModel, destinationFolderViewModel),
                        IFolder folder => new FolderViewModel(folder, browserViewModel, destinationFolderViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(movedItem))
                    }, browserViewModel.Layouts.GetSorter());
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                await transferViewModel.HideAsync();
            }
        }

        /// <summary>
        /// Copies external files from system apps (e.g., Files app) to the destination folder.
        /// </summary>
        /// <param name="dropEventArgs">The drop event args containing the dropped files.</param>
        /// <param name="destinationFolderViewModel">The destination folder view model.</param>
        private static async Task CopyExternalFilesToFolderAsync(DropEventArgs dropEventArgs, FolderViewModel destinationFolderViewModel)
        {
            if (destinationFolderViewModel.Folder is not IModifiableFolder destinationFolder)
                return;

            var browserViewModel = destinationFolderViewModel.BrowserViewModel;
            if (browserViewModel.TransferViewModel is not { IsProgressing: false } transferViewModel)
                return;

            // Get storable items from platform-specific drop data
            var storableItems = new List<IStorable>();

#if IOS || MACCATALYST
            // Get items from PlatformArgs on iOS/macOS
            if (dropEventArgs.PlatformArgs?.DropSession is UIKit.IUIDropSession dropSession)
            {
                var tcs = new TaskCompletionSource<List<Foundation.NSUrl>>();
                var urlList = new List<Foundation.NSUrl>();
                var itemCount = dropSession.Items.Length;
                var processedCount = 0;

                if (itemCount == 0)
                    return;

                foreach (var dragItem in dropSession.Items)
                {
                    var itemProvider = dragItem.ItemProvider;
                    if (itemProvider.HasItemConformingTo(UniformTypeIdentifiers.UTTypes.Item.Identifier))
                    {
                        itemProvider.LoadItem(UniformTypeIdentifiers.UTTypes.Item.Identifier, null, (item, error) =>
                        {
                            if (item is Foundation.NSUrl url)
                            {
                                lock (urlList)
                                {
                                    urlList.Add(url);
                                }
                            }

                            if (Interlocked.Increment(ref processedCount) == itemCount)
                                tcs.TrySetResult(urlList);
                        });
                    }
                    else if (itemProvider.HasItemConformingTo(UniformTypeIdentifiers.UTTypes.FileUrl.Identifier))
                    {
                        itemProvider.LoadItem(UniformTypeIdentifiers.UTTypes.FileUrl.Identifier, null, (item, error) =>
                        {
                            if (item is Foundation.NSUrl url)
                            {
                                lock (urlList)
                                {
                                    urlList.Add(url);
                                }
                            }

                            if (Interlocked.Increment(ref processedCount) == itemCount)
                                tcs.TrySetResult(urlList);
                        });
                    }
                    else
                    {
                        if (Interlocked.Increment(ref processedCount) == itemCount)
                            tcs.TrySetResult(urlList);
                    }
                }

                // Wait for all items to be loaded
                var urls = await tcs.Task;
                foreach (var url in urls)
                {
                    if (url.Path is null)
                        continue;

                    var isDir = false;
                    var isDirectory = Foundation.NSFileManager.DefaultManager.FileExists(url.Path, ref isDir) && isDir;
                    if (isDirectory)
                        storableItems.Add(new IOSFolder(url));
                    else
                        storableItems.Add(new IOSFile(url));
                }
            }
#elif ANDROID
            // TODO: Implement Android drag-and-drop from external apps
#endif

            if (storableItems.Count == 0)
                return;

            try
            {
                transferViewModel.TransferType = TransferType.Copy;
                using var cts = transferViewModel.GetCancellation();

                await transferViewModel.TransferAsync(storableItems, async (item, reporter, token) =>
                {
                    // Copy
                    var copiedItem = await destinationFolder.CreateCopyOfStorableAsync(item, false, reporter, token);

                    // Add to destination
                    destinationFolderViewModel.Items.Insert(copiedItem switch
                    {
                        IFile file => new FileViewModel(file, browserViewModel, destinationFolderViewModel),
                        IFolder folder => new FolderViewModel(folder, browserViewModel, destinationFolderViewModel),
                        _ => throw new ArgumentOutOfRangeException(nameof(copiedItem))
                    }, browserViewModel.Layouts.GetSorter());
                }, cts.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                _ = ex;
                // TODO: Report error
            }
            finally
            {
                await transferViewModel.HideAsync();
            }
        }

        public object? EmptyView
        {
            get => (object?)GetValue(EmptyViewProperty);
            set => SetValue(EmptyViewProperty, value);
        }
        public static readonly BindableProperty EmptyViewProperty =
            BindableProperty.Create(nameof(EmptyView), typeof(object), typeof(BrowserControl), defaultValue: null);

        public bool IsSelecting
        {
            get => (bool)GetValue(IsSelectingProperty);
            set => SetValue(IsSelectingProperty, value);
        }
        public static readonly BindableProperty IsSelectingProperty =
            BindableProperty.Create(nameof(IsSelecting), typeof(bool), typeof(BrowserControl), defaultValue: false);

        public ICommand? RefreshCommand
        {
            get => (ICommand?)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
        public static readonly BindableProperty RefreshCommandProperty =
            BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(BrowserControl), defaultValue: null);

        public IList<BrowserItemViewModel>? ItemsSource
        {
            get => (IList<BrowserItemViewModel>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IList<BrowserItemViewModel>), typeof(BrowserControl), defaultValue: null);

        public BrowserViewType ViewType
        {
            get => (BrowserViewType)GetValue(ViewTypeProperty);
            set => SetValue(ViewTypeProperty, value);
        }
        public static readonly BindableProperty ViewTypeProperty =
            BindableProperty.Create(nameof(ViewType), typeof(BrowserViewType), typeof(BrowserControl), defaultValue: BrowserViewType.ListView);
    }
}
