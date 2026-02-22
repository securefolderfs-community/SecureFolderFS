using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl
    {
        private const double SWIPE_SELECTION_MIN_HORIZONTAL_THRESHOLD = 10.0d;
        private readonly SwipeSelectionManager _swipeSelectionManager = new();
        private Point _swipeOriginCenterPoint;
        private Point? _swipeStartPan;
        private ScrollView? _scrollView;

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
                if (itemViewModel is not FolderViewModel)
                    _skipCollectionViewLayoutPass++;

                await itemViewModel.OpenCommand.ExecuteAsync(null);
                view.IsEnabled = true;
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
                        BeginSelectionRectangle(originView, originItem, e.TotalX, e.TotalY);
                    }

                    UpdateSelectionRectangle(originView, e.TotalX, e.TotalY);
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    _swipeSelectionManager.End();
                    EndSelectionRectangle();
                    break;
            }
        }
        
        private void BeginSelectionRectangle(View originView, BrowserItemViewModel originItem, double totalX, double totalY)
        {
            if (_collectionView is null || ItemsSource is null)
                return;

            GetItemLayout(originView, out var columns, out var itemWidth, out var itemHeight,
                out var hSpacing, out var vSpacing, out var offsetX, out var offsetY);

            var originIndex = ItemsSource.IndexOf(originItem);
            var originCol = originIndex % columns;
            var originRow = originIndex / columns;

            // Get current scroll offset
            double scrollY = GetCurrentScrollY();

            // Center of origin item in CANVAS coordinates (screen coordinates):
            // content Y → canvas Y = contentY - scrollY
            double canvasX = offsetX + originCol * (itemWidth + hSpacing) + itemWidth / 2.0;
            double canvasY = offsetY + originRow * (itemHeight + vSpacing) + itemHeight / 2.0 - scrollY;

            _swipeOriginCenterPoint = new Point(canvasX, canvasY);
            _swipeStartPan = new Point(totalX, totalY);
            SelectionRectangleCanvas.IsVisible = true;
        }

        private void UpdateSelectionRectangle(View originView, double totalX, double totalY)
        {
            if (_collectionView is null || ItemsSource is null || _swipeStartPan is null)
                return;

            GetItemLayout(originView, out var columns, out var itemWidth, out var itemHeight,
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
            double scrollY = GetCurrentScrollY();

            // Transform hit rectangle into CONTENT coordinates by adding scroll offset
            var hitRectContent = new Rect(
                hitRectCanvas.X,
                hitRectCanvas.Y + scrollY,
                hitRectCanvas.Width,
                hitRectCanvas.Height);

            _swipeSelectionManager.UpdateFromRectangle(ItemsSource, item =>
            {
                var index = ItemsSource.IndexOf(item);
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
        }

        private void PositionSelectionRectangle(Rect rect)
        {
            SelectionRectangleView.Margin = new Thickness(rect.X, rect.Y, 0, 0);
            SelectionRectangleView.WidthRequest = rect.Width;
            SelectionRectangleView.HeightRequest = rect.Height;
        }

        private void GetItemLayout(View originView, out int columns, out double itemWidth, out double itemHeight,
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
            itemHeight = columns == 1 ? originView.Height : itemWidth;
        }

        /// <summary>Retrieves the current vertical scroll offset of the CollectionView.</summary>
        private double GetCurrentScrollY()
        {
            if (_collectionView == null)
                return 0;

            // Lazy‑load the internal ScrollView
            if (_scrollView == null)
            {
                _scrollView = GetInternalScrollView(_collectionView);
            }

            return _scrollView?.ScrollY ?? 0;
        }

        /// <summary>Finds the internal ScrollView of a CollectionView via the visual tree.</summary>
        private static ScrollView? GetInternalScrollView(CollectionView collectionView)
        {
            if (collectionView is not IVisualTreeElement vte)
                return null;

            // Search the first‑level children – in practice the ScrollView is a direct child.
            foreach (var child in vte.GetVisualChildren())
            {
                if (child is ScrollView sv)
                    return sv;
            }
            return null;
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

        private void UpdateItemContainerPanGesture(View view)
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
    }
}
