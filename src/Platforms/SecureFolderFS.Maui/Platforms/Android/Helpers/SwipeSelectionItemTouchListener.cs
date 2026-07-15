using Android.Views;
using AndroidX.RecyclerView.Widget;
using SecureFolderFS.Maui.UserControls.Browser;

namespace SecureFolderFS.Maui.Platforms.Android.Helpers
{
    /// <summary>
    /// Implements swipe-to-select on Android by claiming the gesture at the RecyclerView level.
    /// </summary>
    /// <remarks>
    /// MAUI gesture recognizers cannot drive this feature on Android: a PanGestureRecognizer
    /// conflicts with the per-item TapGestureRecognizer, and the gesture is lost mid-way to the
    /// RecyclerView's own scroll interception and to SwipeRefreshLayout (RefreshView). An
    /// <see cref="RecyclerView.IOnItemTouchListener"/> is consulted BEFORE the RecyclerView's own
    /// touch handling, so once horizontal intent is detected the gesture can be claimed for
    /// selection - blocking scrolling for its duration - while purely vertical gestures are left
    /// untouched and scroll the list normally. Taps and long-presses (context menu) never move
    /// past the intent threshold and keep working unchanged.
    /// </remarks>
    internal sealed class SwipeSelectionItemTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
    {
        private readonly BrowserControl _browserControl;
        private float _downX;
        private float _downY;
        private bool _isTracking;
        private bool _isSelectionActive;

        public SwipeSelectionItemTouchListener(BrowserControl browserControl)
        {
            _browserControl = browserControl;
        }

        /// <inheritdoc/>
        public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
        {
            if (!_browserControl.IsSelecting)
                return false;

            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                {
                    _downX = e.GetX();
                    _downY = e.GetY();
                    _isTracking = true;
                    _isSelectionActive = false;
                    break;
                }

                case MotionEventActions.Move when _isTracking && !_isSelectionActive:
                {
                    var density = GetDensity(rv);
                    var totalX = (e.GetX() - _downX) / density;
                    var totalY = (e.GetY() - _downY) / density;
                    var absX = Math.Abs(totalX);
                    var absY = Math.Abs(totalY);

                    // Vertical intent - stop tracking and let the RecyclerView scroll
                    if (absY > absX && absY > BrowserControl.SWIPE_SELECTION_MIN_HORIZONTAL_THRESHOLD)
                    {
                        _isTracking = false;
                        return false;
                    }

                    if (absX < BrowserControl.SWIPE_SELECTION_MIN_HORIZONTAL_THRESHOLD || absY > absX)
                        return false;

                    // Horizontal intent confirmed - resolve the item under the initial touch
                    var child = rv.FindChildViewUnder(_downX, _downY);
                    var originIndex = child is null ? RecyclerView.NoPosition : rv.GetChildAdapterPosition(child);
                    if (child is null || originIndex == RecyclerView.NoPosition)
                    {
                        _isTracking = false;
                        return false;
                    }

                    if (!_browserControl.TryBeginPlatformSwipeSelection(originIndex, child.Height / density, totalX, totalY))
                    {
                        _isTracking = false;
                        return false;
                    }

                    // Claim the gesture: subsequent events arrive in OnTouchEvent, and parents
                    // (e.g. SwipeRefreshLayout backing RefreshView) may no longer intercept it
                    _isSelectionActive = true;
                    rv.Parent?.RequestDisallowInterceptTouchEvent(true);
                    return true;
                }

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                {
                    _isTracking = false;
                    break;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public void OnTouchEvent(RecyclerView rv, MotionEvent e)
        {
            if (!_isSelectionActive)
                return;

            switch (e.ActionMasked)
            {
                case MotionEventActions.Move:
                {
                    var density = GetDensity(rv);
                    _browserControl.UpdatePlatformSwipeSelection(
                        (e.GetX() - _downX) / density,
                        (e.GetY() - _downY) / density);
                    break;
                }

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                {
                    _isSelectionActive = false;
                    _isTracking = false;
                    rv.Parent?.RequestDisallowInterceptTouchEvent(false);
                    _browserControl.EndPlatformSwipeSelection();
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
        {
        }

        private static float GetDensity(RecyclerView rv)
        {
            var density = rv.Resources?.DisplayMetrics?.Density ?? 1f;
            return density > 0f ? density : 1f;
        }
    }
}
