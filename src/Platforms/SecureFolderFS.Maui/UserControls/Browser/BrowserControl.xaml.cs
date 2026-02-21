using System.Windows.Input;
using APES.UI.XF;
using OwlCore.Storage;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Maui.ValueConverters;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.UI;

#if IOS || MACCATALYST
using Foundation;
using UniformTypeIdentifiers;
#endif

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl : ContentView
    {
        private readonly DeferredInitialization<IFolder> _deferredInitialization;
        private readonly ISettingsService _settingsService;
        private int _skipCollectionViewLayoutPass;
        private CollectionView? _collectionView;

        public BrowserControl()
        {
            _deferredInitialization = new(Constants.Browser.THUMBNAIL_MAX_PARALLELISATION);
            _settingsService = DI.Service<ISettingsService>();
            InitializeComponent();
        }

        /// <summary>
        /// Determines if the CollectionView can be reloaded.
        /// </summary>
        /// <returns>Returns true if reloading is possible; otherwise, false.</returns>
        public bool CanReloadCollection()
        {
            return _skipCollectionViewLayoutPass == 0;
        }

        /// <summary>
        /// Forces complete recreation of the CollectionView to work around MAUI layout glitches
        /// when changing ItemsLayout dynamically.
        /// </summary>
        public async Task ReloadCollectionViewAsync()
        {
            if (_skipCollectionViewLayoutPass > 0)
            {
                _skipCollectionViewLayoutPass--;
                return;
            }
            
            if (_collectionView is null)
                return;

            // Find the parent container
            var container = CollectionViewContainer;
            if (container is null)
                return;

            // Fade out
            await _collectionView.FadeToAsync(0, 100);

            // Store references to templates and empty view before removing
            var itemTemplate = _collectionView.ItemTemplate;
            var emptyView = _collectionView.EmptyView;

            // Remove the old CollectionView
            container.Children.Remove(_collectionView);

            // Create a brand new CollectionView
            var newCollectionView = new CollectionView()
            {
                ItemsLayout = ViewTypeToItemsLayoutConverter.ConvertLayout(ViewType),
                ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
                ItemTemplate = itemTemplate,
                EmptyView = emptyView,
                Opacity = 0
            };

            // Set up bindings
            newCollectionView.SetBinding(ItemsView.ItemsSourceProperty,
                new Binding(nameof(ItemsSource), mode: BindingMode.OneWay, source: this));
            newCollectionView.SetBinding(IsVisibleProperty,
                new Binding($"{nameof(ItemsSource)}.Count", mode: BindingMode.OneWay, source: this,
                    converter: GetConverter("CountToBoolConverter")));
            newCollectionView.SetBinding(SelectableItemsView.SelectionModeProperty,
                new Binding(nameof(IsSelecting), mode: BindingMode.OneWay, source: this,
                    converter: GetConverter("BoolSelectionModeConverter")));

            // Wire up the events
            newCollectionView.Loaded += ItemsCollectionView_Loaded;
            newCollectionView.SizeChanged += ItemsCollectionView_SizeChanged;

            // Add the new CollectionView to the container
            container.Children.Add(newCollectionView);

            // Update our reference
            _collectionView = newCollectionView;

            // Wait for layout
            await Task.Delay(50);

            // Fade in
            await _collectionView.FadeToAsync(1, 100);

            // Re-trigger thumbnail loading for visible items
            EnqueueVisibleItemsForThumbnails();
        }

        private void EnqueueVisibleItemsForThumbnails()
        {
            if (!_settingsService.UserSettings.AreThumbnailsEnabled || ItemsSource is null)
                return;

            if (ItemsSource.FirstOrDefault() is not { ParentFolder.Folder: { } folder})
                return;
            
            _deferredInitialization.SetContext(folder);
            foreach (var item in ItemsSource.OfType<FileViewModel>().Where(f => f.CanLoadThumbnail))
                _deferredInitialization.Enqueue(item);
        }
        
        private void TryEnqueueThumbnail(object? sender)
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
        
        private static IValueConverter? GetConverter(string key)
        {
            // Try local resources first, then app resources
            if (Application.Current?.Resources.TryGetValue(key, out var converter) == true)
                return converter as IValueConverter;
            
            return null;
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
                if (itemViewModel is not FolderViewModel)
                    _skipCollectionViewLayoutPass++;
                
                await itemViewModel.OpenCommand.ExecuteAsync(null);
                view.IsEnabled = true;
            }
        }

        private void ItemContainer_Loaded(object? sender, EventArgs e)
        {
            TryEnqueueThumbnail(sender);
        }

        private void ItemContainer_BindingContextChanged(object? sender, EventArgs e)
        {
            // Also handle BindingContextChanged for virtualized/recycled items on iOS
            TryEnqueueThumbnail(sender);
        }
        
        private void ItemsCollectionView_Loaded(object? sender, EventArgs e)
        {
            _collectionView = sender as CollectionView;
            
            // Set initial ItemsLayout since we removed the binding from XAML
            _collectionView?.ItemsLayout = ViewTypeToItemsLayoutConverter.ConvertLayout(ViewType);
        }

        private void ItemsCollectionView_SizeChanged(object? sender, EventArgs e)
        {
            // Force layout recalculation when the collection view size changes
            // This helps ensure proper item sizing after orientation changes
            _collectionView?.InvalidateMeasure();
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

#if IOS || MACCATALYST
            // Get items from PlatformArgs on iOS/macOS
            if (dropEventArgs.PlatformArgs?.DropSession is { } dropSession)
            {
                var itemCount = dropSession.Items.Length;
                if (itemCount == 0)
                    return;

                // Collect all items to process - either as file URLs or as data streams
                var itemsToProcess = new List<(string Name, Func<CancellationToken, Task<Stream?>> DataLoader, bool IsFolder)>();

                foreach (var dragItem in dropSession.Items)
                {
                    var itemProvider = dragItem.ItemProvider;
                    var suggestedName = itemProvider.SuggestedName ?? "Unknown";

                    // First, check for Gallery/Photos app items (specific image/video types loaded as data)
                    // These need to be checked first because UTTypes.Item would also match them
                    var galleryTypeIdentifiers = new[]
                    {
                        UTTypes.Jpeg.Identifier,
                        UTTypes.Png.Identifier,
                        UTTypes.Heic.Identifier,
                        UTTypes.Gif.Identifier,
                        UTTypes.Mpeg4Movie.Identifier,
                        UTTypes.QuickTimeMovie.Identifier,
                        UTTypes.Image.Identifier,
                        UTTypes.Movie.Identifier
                    };

                    // Check if this is a Gallery item by seeing if it does NOT have a FileUrl representation
                    // Files app items have FileUrl, Gallery items don't
                    var matchedGalleryTypeIdentifier = galleryTypeIdentifiers.FirstOrDefault(typeId => itemProvider.HasItemConformingTo(typeId));
                    var hasFileUrl = itemProvider.HasItemConformingTo(UTTypes.FileUrl.Identifier);

                    if (matchedGalleryTypeIdentifier is not null && !hasFileUrl)
                    {
                        // This is a Gallery/Photos app item - load as data representation
                        var capturedTypeId = matchedGalleryTypeIdentifier;
                        var capturedProvider = itemProvider;
                        
                        // Determine extension from type identifier
                        var extension = GetExtensionFromTypeIdentifier(capturedTypeId);
                        var suggestedExtension = Path.GetExtension(suggestedName);
                        var actualName = suggestedName;
                        if (string.IsNullOrEmpty(suggestedExtension) && !string.IsNullOrEmpty(extension))
                            actualName = suggestedName + extension;

                        itemsToProcess.Add((actualName, async _ =>
                        {
                            var dataTcs = new TaskCompletionSource<NSData?>();
                            var utType = UTType.CreateFromIdentifier(capturedTypeId);
                            if (utType is null)
                                return null;
                                
                            capturedProvider.LoadDataRepresentation(utType, (data, _) =>
                            {
                                dataTcs.TrySetResult(data);
                            });

                            var data = await dataTcs.Task;
                            return data?.AsStream();
                        }, false));
                        
                        continue;
                    }

                    // Second, try to load as a file URL (works for Files app)
                    if (itemProvider.HasItemConformingTo(UTTypes.Item.Identifier))
                    {
                        var tcs = new TaskCompletionSource<(NSUrl? Url, bool IsFolder)>();
                        itemProvider.LoadItem(UTTypes.Item.Identifier, null, (item, _) =>
                        {
                            if (item is NSUrl { Path: not null } itemUrl)
                            {
                                var isDir = false;
                                var isDirectory = NSFileManager.DefaultManager.FileExists(itemUrl.Path, ref isDir) && isDir;
                                tcs.TrySetResult((itemUrl, isDirectory));
                            }
                            else
                            {
                                tcs.TrySetResult((null, false));
                            }
                        });

                        var (url, isFolder) = await tcs.Task;
                        if (url is not null)
                        {
                            // Get the actual filename from the URL path - this should include the correct extension
                            var fileNameFromPath = Path.GetFileName(url.Path!);
                            
                            // Use the filename from path if available (it has the correct extension),
                            // otherwise fall back to suggestedName with extension appended
                            string actualName;
                            if (!string.IsNullOrEmpty(fileNameFromPath) && !string.IsNullOrEmpty(Path.GetExtension(fileNameFromPath)))
                            {
                                // URL path has filename with extension - use it directly
                                actualName = fileNameFromPath;
                            }
                            else
                            {
                                // Fall back to suggested name, appending extension from path if needed
                                var pathExtension = Path.GetExtension(url.Path!);
                                var suggestedExtension = Path.GetExtension(suggestedName);
                                actualName = string.IsNullOrEmpty(suggestedExtension) && !string.IsNullOrEmpty(pathExtension)
                                    ? suggestedName + pathExtension
                                    : suggestedName;
                            }

                            if (isFolder)
                            {
                                itemsToProcess.Add((actualName, _ => Task.FromResult<Stream?>(null), true));
                            }
                            else
                            {
                                var capturedUrl = url;
                                itemsToProcess.Add((actualName, async ct =>
                                {
                                    var accessStarted = capturedUrl.StartAccessingSecurityScopedResource();
                                    try
                                    {
                                        if (capturedUrl.Path is not null && File.Exists(capturedUrl.Path))
                                        {
                                            var ms = new MemoryStream();
                                            await using var fs = File.OpenRead(capturedUrl.Path);
                                            await fs.CopyToAsync(ms, ct);
                                            ms.Position = 0;
                                            return ms;
                                        }
                                    }
                                    finally
                                    {
                                        if (accessStarted)
                                            capturedUrl.StopAccessingSecurityScopedResource();
                                    }
                                    return null;
                                }, false));
                            }
                            continue;
                        }
                    }

                    // Fall back to loading as generic data
                    if (itemProvider.HasItemConformingTo(UTTypes.Data.Identifier))
                    {
                        var capturedProvider = itemProvider;
                        itemsToProcess.Add((suggestedName, async _ =>
                        {
                            var dataTcs = new TaskCompletionSource<NSData?>();
                            capturedProvider.LoadDataRepresentation(UTTypes.Data, (data, _) =>
                            {
                                dataTcs.TrySetResult(data);
                            });

                            var data = await dataTcs.Task;
                            return data?.AsStream();
                        }, false));
                    }
                }

                if (itemsToProcess.Count == 0)
                    return;

                try
                {
                    transferViewModel.TransferType = TransferType.Copy;
                    using var cts = transferViewModel.GetCancellation();

                    await transferViewModel.TransferAsync(itemsToProcess, async (item, reporter, token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        if (item.IsFolder)
                        {
                            // Create folder
                            var createdFolder = await destinationFolder.CreateFolderAsync(item.Name, false, token);
                            destinationFolderViewModel.Items.Insert(
                                new FolderViewModel(createdFolder, browserViewModel, destinationFolderViewModel),
                                browserViewModel.Layouts.GetSorter());
                        }
                        else
                        {
                            // Load data and create file
                            await using var dataStream = await item.DataLoader(token);
                            if (dataStream is null)
                                return;

                            var createdFile = await destinationFolder.CreateFileAsync(item.Name, false, token);
                            await using var destinationStream = await createdFile.OpenStreamAsync(FileAccess.Write, token);
                            await dataStream.CopyToAsync(destinationStream, token);
                            reporter.Report(createdFile);

                            destinationFolderViewModel.Items.Insert(
                                new FileViewModel(createdFile, browserViewModel, destinationFolderViewModel),
                                browserViewModel.Layouts.GetSorter());
                        }
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
#elif ANDROID
            // TODO: Implement Android drag-and-drop from external apps
#endif
        }

#if IOS || MACCATALYST
        private static string GetExtensionFromTypeIdentifier(string typeIdentifier)
        {
            return typeIdentifier switch
            {
                _ when typeIdentifier == UTTypes.Jpeg.Identifier => ".jpg",
                _ when typeIdentifier == UTTypes.Png.Identifier => ".png",
                _ when typeIdentifier == UTTypes.Heic.Identifier => ".heic",
                _ when typeIdentifier == UTTypes.Gif.Identifier => ".gif",
                _ when typeIdentifier == UTTypes.Mpeg4Movie.Identifier => ".mp4",
                _ when typeIdentifier == UTTypes.QuickTimeMovie.Identifier => ".mov",
                _ when typeIdentifier == UTTypes.Tiff.Identifier => ".tiff",
                _ when typeIdentifier == UTTypes.Bmp.Identifier => ".bmp",
                _ when typeIdentifier == UTTypes.Pdf.Identifier => ".pdf",
                _ => string.Empty
            };
        }
#endif

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
