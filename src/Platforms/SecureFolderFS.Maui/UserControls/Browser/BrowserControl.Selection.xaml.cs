using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl
    {
        private const double SWIPE_SELECTION_MIN_HORIZONTAL_THRESHOLD = 10.0d;
        private readonly SwipeSelectionManager _swipeSelectionManager = new();
        private Point _swipeOriginCenterPoint; // Center of origin cell - for hit-testing
        private Point? _swipeStartPan; // pan coordinates at the very first Running event

        private async void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not View { BindingContext: BrowserItemViewModel itemViewModel } view)
                return;

            // Ignore taps that are part of a swipe-select gesture
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
            // Only active while in selection mode
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
                    // Don't begin yet — wait until we confirm horizontal intent in Running
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

            // Center of the origin item in canvas coordinates (including padding)
            _swipeOriginCenterPoint = new Point(
                offsetX + originCol * (itemWidth + hSpacing) + itemWidth / 2.0,
                offsetY + originRow * (itemHeight + vSpacing) + itemHeight / 2.0);

            _swipeStartPan = new Point(totalX, totalY);
            SelectionRectangleCanvas.IsVisible = true;
        }

        private void UpdateSelectionRectangle(View originView, double totalX, double totalY)
        {
            if (_collectionView is null || ItemsSource is null || _swipeStartPan is null)
                return;

            GetItemLayout(originView, out var columns, out var itemWidth, out var itemHeight,
                out var hSpacing, out var vSpacing, out var offsetX, out var offsetY);

            // Delta from the moment the swipe was confirmed
            var deltaX = totalX - _swipeStartPan.Value.X;
            var deltaY = totalY - _swipeStartPan.Value.Y;

            // Current finger position = start point + delta
            var startPoint = _swipeOriginCenterPoint;
            var currentPoint = new Point(startPoint.X + deltaX, startPoint.Y + deltaY);

            // Rectangle bounds in canvas coordinates
            var rectX = Math.Min(startPoint.X, currentPoint.X);
            var rectY = Math.Min(startPoint.Y, currentPoint.Y);
            var rectW = Math.Abs(currentPoint.X - startPoint.X);
            var rectH = Math.Abs(currentPoint.Y - startPoint.Y);

            PositionSelectionRectangle(new Rect(rectX, rectY, rectW, rectH));

            // Hit-testing – build item rectangles with padding offset
            for (var i = 0; i < ItemsSource.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var itemRect = new Rect(
                    offsetX + col * (itemWidth + hSpacing),
                    offsetY + row * (itemHeight + vSpacing),
                    itemWidth,
                    itemHeight);

                var hitRect = new Rect(
                    Math.Min(startPoint.X, currentPoint.X),
                    Math.Min(startPoint.Y, currentPoint.Y),
                    Math.Abs(currentPoint.X - startPoint.X),
                    Math.Abs(currentPoint.Y - startPoint.Y));

                if (hitRect.IntersectsWith(itemRect))
                    _swipeSelectionManager.Update(ItemsSource[i]);
            }
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

        /// <summary>Extracts layout metrics shared by Begin and Update</summary>
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

            // Available width = total width minus horizontal margins AND horizontal padding
            var availableWidth = _collectionView.Width - _collectionView.Margin.Left - _collectionView.Margin.Right;

            itemWidth = (availableWidth - hSpacing * (columns - 1)) / columns;
            itemHeight = columns == 1 ? originView.Height : itemWidth;
        }

        /// <summary>
        /// Attaches a PanGestureRecognizer to the item container (if not already present)
        /// and records the rendered view on the view model so hit-testing can find it.
        /// </summary>
        private void RegisterItemContainerPanGesture(object? sender)
        {
            if (sender is not View { BindingContext: BrowserItemViewModel } view)
                return;

            // Avoid adding duplicate recognizers on recycled cells
            if (view.GestureRecognizers.OfType<PanGestureRecognizer>().Any())
                return;

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += ItemContainer_PanUpdated;
            view.GestureRecognizers.Add(pan);
        }
    }
}
