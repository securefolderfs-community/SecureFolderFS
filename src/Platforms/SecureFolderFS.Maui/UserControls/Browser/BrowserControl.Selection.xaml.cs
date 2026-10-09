using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl
    {
        internal const double SWIPE_SELECTION_MIN_HORIZONTAL_THRESHOLD = 10d;

        private readonly SwipeSelectionManager _swipeSelectionManager = new();
        private Point _swipeOriginCenterPoint;
        private Point? _swipeStartPan;
        private double _swipeOriginItemHeight;
        private double _currentScrollY;
        private Dictionary<BrowserItemViewModel, int>? _swipeIndexMap;

        private async void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not View { BindingContext: BrowserItemViewModel itemViewModel } view)
                return;

            if (_swipeSelectionManager.IsActive)
                return;

            if (IsSelecting)
                itemViewModel.IsSelected = !itemViewModel.IsSelected;
            else
            {
                view.IsEnabled = false;
                var skipReload = itemViewModel is not FolderViewModel;
                if (skipReload)
                    _skipCollectionViewLayoutPass++;

                try
                {
                    await itemViewModel.OpenCommand.ExecuteAsync(null);
                }
                catch (Exception)
                {
                    // The open failed, so no navigation will consume the skipped layout pass
                    if (skipReload && _skipCollectionViewLayoutPass > 0)
                        _skipCollectionViewLayoutPass--;
                }
                finally
                {
                    view.IsEnabled = true;
                }
            }
        }

        internal void ItemContainer_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (!IsSelecting)
            {
                _swipeSelectionManager.End();
                return;
            }

            if (sender is not View { BindingContext: BrowserItemViewModel originItem } originView)
                return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    // Wait for horizontal intent in Running
                    break;

                case GestureStatus.Running:
                    if (!IsSelecting && !_swipeSelectionManager.IsActive)
                        return;

                    if (!_swipeSelectionManager.IsActive)
                    {
                        var absX = Math.Abs(e.TotalX);
                        var absY = Math.Abs(e.TotalY);

                        if (absX < SWIPE_SELECTION_MIN_HORIZONTAL_THRESHOLD || absY > absX)
                            return;

                        _swipeSelectionManager.Begin(originItem);
                        BeginSelectionRectangle(originItem, originView.Height, e.TotalX, e.TotalY);
                    }

                    UpdateSelectionRectangle(e.TotalX, e.TotalY);
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    _swipeSelectionManager.End();
                    EndSelectionRectangle();
                    break;
            }
        }

        /// <summary>
        /// Begins a swipe selection driven by platform (native) touch events. Used on Android,
        /// where the gesture is claimed at the RecyclerView level instead of via MAUI gestures.
        /// </summary>
        /// <param name="originIndex">The index of the item the gesture started on.</param>
        /// <param name="originItemHeight">The height of the origin item, in device-independent units.</param>
        /// <param name="totalX">The horizontal pan total at the time selection was claimed.</param>
        /// <param name="totalY">The vertical pan total at the time selection was claimed.</param>
        /// <returns>True when the selection was started; otherwise false.</returns>
        internal bool TryBeginPlatformSwipeSelection(int originIndex, double originItemHeight, double totalX, double totalY)
        {
            if (!IsSelecting || ItemsSource is null)
                return false;

            if (originIndex < 0 || originIndex >= ItemsSource.Count)
                return false;

            var originItem = ItemsSource[originIndex];
            _swipeSelectionManager.Begin(originItem);
            BeginSelectionRectangle(originItem, originItemHeight, totalX, totalY);

            return true;
        }

        /// <summary>
        /// Updates a swipe selection started with <see cref="TryBeginPlatformSwipeSelection"/>.
        /// </summary>
        internal void UpdatePlatformSwipeSelection(double totalX, double totalY)
        {
            if (!_swipeSelectionManager.IsActive)
                return;

            UpdateSelectionRectangle(totalX, totalY);
        }

        /// <summary>
        /// Ends a swipe selection started with <see cref="TryBeginPlatformSwipeSelection"/>.
        /// </summary>
        internal void EndPlatformSwipeSelection()
        {
            _swipeSelectionManager.End();
            EndSelectionRectangle();
        }

        private void BeginSelectionRectangle(BrowserItemViewModel originItem, double originItemHeight, double totalX, double totalY)
        {
            if (_collectionView is null || ItemsSource is null)
                return;

            _swipeOriginItemHeight = originItemHeight;
            GetItemLayout(out var columns, out var itemWidth, out var itemHeight,
                out var hSpacing, out var vSpacing, out var offsetX, out var offsetY);

            // Snapshot item positions once per gesture - looking indices up per item on
            // every pan update would be quadratic in the number of items
            _swipeIndexMap = new Dictionary<BrowserItemViewModel, int>(ItemsSource.Count);
            for (var i = 0; i < ItemsSource.Count; i++)
                _swipeIndexMap[ItemsSource[i]] = i;

            var originIndex = _swipeIndexMap.GetValueOrDefault(originItem, 0);
            var originCol = originIndex % columns;
            var originRow = originIndex / columns;

            // Get the current scroll offset
            var scrollY = GetCurrentScrollY();

            // Center of origin item in CANVAS coordinates (screen coordinates):
            // content Y = canvas Y = contentY - scrollY
            var canvasX = offsetX + originCol * (itemWidth + hSpacing) + itemWidth / 2d;
            var canvasY = offsetY + originRow * (itemHeight + vSpacing) + itemHeight / 2d - scrollY;

            _swipeOriginCenterPoint = new Point(canvasX, canvasY);
            _swipeStartPan = new Point(totalX, totalY);
            SelectionRectangleCanvas.IsVisible = true;
        }

        private void UpdateSelectionRectangle(double totalX, double totalY)
        {
            if (_collectionView is null || ItemsSource is null || _swipeStartPan is null)
                return;

            GetItemLayout(out var columns, out var itemWidth, out var itemHeight,
                out var hSpacing, out var vSpacing, out var offsetX, out var offsetY);

            var deltaX = totalX - _swipeStartPan.Value.X;
            var deltaY = totalY - _swipeStartPan.Value.Y;

            var startPoint = _swipeOriginCenterPoint;
            var currentPoint = new Point(startPoint.X + deltaX, startPoint.Y + deltaY);

            // Hit rectangle in CANVAS coordinates
            var hitRectCanvas = new Rect(
                Math.Min(startPoint.X, currentPoint.X),
                Math.Min(startPoint.Y, currentPoint.Y),
                Math.Abs(currentPoint.X - startPoint.X),
                Math.Abs(currentPoint.Y - startPoint.Y));

            PositionSelectionRectangle(hitRectCanvas);

            // Get current scroll offset again (it might have changed during the gesture)
            var scrollY = GetCurrentScrollY();

            // Transform hit rectangle into CONTENT coordinates by adding scroll offset
            var hitRectContent = new Rect(
                hitRectCanvas.X,
                hitRectCanvas.Y + scrollY,
                hitRectCanvas.Width,
                hitRectCanvas.Height);

            _swipeSelectionManager.UpdateFromRectangle(ItemsSource, item =>
            {
                if (_swipeIndexMap is null || !_swipeIndexMap.TryGetValue(item, out var index))
                    return false;

                var col = index % columns;
                var row = index / columns;

                // Item rectangle in CONTENT coordinates (as before)
                var itemRect = new Rect(
                    offsetX + col * (itemWidth + hSpacing),
                    offsetY + row * (itemHeight + vSpacing),
                    itemWidth,
                    itemHeight);

                return hitRectContent.IntersectsWith(itemRect);
            });
        }

        private void EndSelectionRectangle()
        {
            SelectionRectangleCanvas.IsVisible = false;
            _swipeStartPan = null;
            _swipeIndexMap = null;
        }

        private void PositionSelectionRectangle(Rect rect)
        {
            SelectionRectangleView.Margin = new Thickness(rect.X, rect.Y, 0, 0);
            SelectionRectangleView.WidthRequest = rect.Width;
            SelectionRectangleView.HeightRequest = rect.Height;
        }

        private void GetItemLayout(out int columns, out double itemWidth, out double itemHeight,
            out double hSpacing, out double vSpacing, out double contentOffsetX, out double contentOffsetY)
        {
            hSpacing = 0;
            vSpacing = 0;
            contentOffsetX = _collectionView.Margin.Left;
            contentOffsetY = _collectionView.Margin.Top;

            if (_collectionView.ItemsLayout is GridItemsLayout gridLayout)
            {
                columns = gridLayout.Span;
                hSpacing = gridLayout.HorizontalItemSpacing;
                vSpacing = gridLayout.VerticalItemSpacing;
            }
            else
                columns = 1;

            var availableWidth = _collectionView.Width - _collectionView.Margin.Left - _collectionView.Margin.Right;
            itemWidth = (availableWidth - hSpacing * (columns - 1)) / columns;
            itemHeight = columns == 1 ? _swipeOriginItemHeight : itemWidth;
        }

        /// <summary>Retrieves the current vertical scroll offset of the CollectionView, in device-independent units.</summary>
        /// <remarks>
        /// The offset is tracked via the Scrolled event - the CollectionView is backed by a native
        /// list (UICollectionView/RecyclerView), so there is no MAUI ScrollView in its visual tree to query.
        /// </remarks>
        private double GetCurrentScrollY()
        {
            return _currentScrollY;
        }

        private void ItemsCollectionView_Scrolled(object? sender, ItemsViewScrolledEventArgs e)
        {
#if ANDROID
            // On Android the CollectionView reports scroll offsets in pixels
            _currentScrollY = e.VerticalOffset / DeviceDisplay.MainDisplayInfo.Density;
#else
            _currentScrollY = e.VerticalOffset;
#endif
        }

        private void RegisterItemContainerPanGesture(object? sender)
        {
            if (sender is not View { BindingContext: BrowserItemViewModel } view)
                return;
            UpdateItemContainerPanGesture(view);
        }

        internal void UpdateAllItemContainerPanGestures()
        {
            if (_collectionView is null)
                return;

            foreach (var view in _collectionView.GetVisualTreeDescendants().OfType<View>().Where(v => v.BindingContext is BrowserItemViewModel))
                UpdateItemContainerPanGesture(view);
        }

        private void UpdateItemContainerPanGesture(
#if ANDROID
            View _)
        {
            // On Android, MAUI's PanGestureRecognizer conflicts with the TapGestureRecognizer and
            // loses the gesture to RecyclerView scrolling and SwipeRefreshLayout interception.
            // Swipe-selection is instead implemented natively at the RecyclerView level
            // (see SwipeSelectionItemTouchListener, attached in BrowserControl.Rendering)
        }
#else
            View view)
        {
            var existing = view.GestureRecognizers.OfType<PanGestureRecognizer>().FirstOrDefault();
            if (IsSelecting)
            {
                if (existing is not null)
                    return;

                var pan = new PanGestureRecognizer();
                pan.PanUpdated += ItemContainer_PanUpdated;
                view.GestureRecognizers.Add(pan);
            }
            else
            {
                if (existing is null)
                    return;

                existing.PanUpdated -= ItemContainer_PanUpdated;
                view.GestureRecognizers.Remove(existing);
            }
        }
#endif
    }
}
