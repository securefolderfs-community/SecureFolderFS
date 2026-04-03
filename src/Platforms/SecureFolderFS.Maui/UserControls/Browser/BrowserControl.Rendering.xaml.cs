using SecureFolderFS.Maui.ValueConverters;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl
    {
        private readonly SemaphoreSlim _thumbnailSemaphore;
        private CancellationTokenSource? _thumbnailCts;
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
#if ANDROID
            // On Android, keep SelectionMode as None to prevent CollectionView re-layout
            // when entering selection mode. Selection is managed via BrowserItemViewModel.IsSelected.
            newCollectionView.SelectionMode = SelectionMode.None;
#else
            newCollectionView.SetBinding(SelectableItemsView.SelectionModeProperty,
                new Binding(nameof(IsSelecting), mode: BindingMode.OneWay, source: this,
                    converter: GetConverter("BoolSelectionModeConverter")));
#endif

            // Wire up the events
            newCollectionView.Loaded += ItemsCollectionView_Loaded;
            newCollectionView.SizeChanged += ItemsCollectionView_SizeChanged;

            // Add the new CollectionView to the container
            container.Children.Add(newCollectionView);

            // Update our reference
            _collectionView = newCollectionView;

            // Fade in
            await _collectionView.FadeToAsync(1, 100);
        }

        private void TryEnqueueItem(object? sender)
        {
            if (!_settingsService.UserSettings.AreThumbnailsEnabled)
                return;

            if (sender is not BindableObject { BindingContext: IAsyncInitialize asyncInitialize })
                return;

            var ct = _thumbnailCts?.Token ?? CancellationToken.None;
            _ = Task.Run(async () =>
            {
                await _thumbnailSemaphore.WaitAsync(ct);
                try
                {
                    await asyncInitialize.InitAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    // Navigation occurred or load was canceled
                }
                finally
                {
                    _thumbnailSemaphore.Release();
                }
            }, ct);
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
            TryEnqueueItem(sender);
            RegisterItemContainerPanGesture(sender);

            if (sender is View view)
                RegisterAndroidDragThreshold(view);
        }

        private void ItemContainer_BindingContextChanged(object? sender, EventArgs e)
        {
#if IOS
            // Also handle BindingContextChanged for virtualized/recycled items on iOS
            TryEnqueueItem(sender);
#endif
        }

        private void ItemsCollectionView_Loaded(object? sender, EventArgs e)
        {
            _collectionView = sender as CollectionView;

            // Set initial ItemsLayout since we removed the binding from XAML
            _collectionView?.ItemsLayout = ViewTypeToItemsLayoutConverter.ConvertLayout(ViewType);

#if ANDROID
            // On Android, keep SelectionMode as None to prevent CollectionView re-layout
            // when entering selection mode. Selection is managed via BrowserItemViewModel.IsSelected.
            if (_collectionView is not null)
                _collectionView.SelectionMode = SelectionMode.None;
#else
            // On other platforms, bind SelectionMode to IsSelecting
            _collectionView?.SetBinding(SelectableItemsView.SelectionModeProperty,
                new Binding(nameof(IsSelecting), mode: BindingMode.OneWay, source: this,
                    converter: GetConverter("BoolSelectionModeConverter")));
#endif
        }

        private void ItemsCollectionView_SizeChanged(object? sender, EventArgs e)
        {
            // Force layout recalculation when the collection view size changes
            // This helps ensure proper item sizing after orientation changes
            _collectionView?.InvalidateMeasure();
        }
    }
}
