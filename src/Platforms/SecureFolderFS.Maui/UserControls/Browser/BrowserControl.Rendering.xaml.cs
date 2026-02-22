using OwlCore.Storage;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Maui.ValueConverters;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl
    {
        private readonly DeferredInitialization<IFolder> _deferredInitialization;
        private readonly ISettingsService _settingsService;
        private int _skipCollectionViewLayoutPass;
        private CollectionView? _collectionView;
        
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
                Margin = ViewType is BrowserViewType.SmallGridView or BrowserViewType.MediumGridView or BrowserViewType.LargeGridView
                    ? new(16d)
                    : new (0d),
                EmptyView = emptyView,
                ZIndex = 0,
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
        
        private void ItemContainer_Loaded(object? sender, EventArgs e)
        {
            TryEnqueueThumbnail(sender);
            RegisterItemContainerPanGesture(sender);
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
    }
}
