using Android.Content;
using Android.Views;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using View = Android.Views.View;

namespace SecureFolderFS.Maui.Platforms.Android.Helpers
{
    /// <summary>
    /// An Android touch handler that implements drag-and-drop with a distance threshold.
    /// This allows the context menu (long-press via <c>ContextMenuContainer</c>) to work
    /// properly on Android by only starting a drag when the finger moves beyond the system's
    /// touch slop threshold, rather than competing with the long-click handler.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="View.Touch"/> C# event (additive) instead of
    /// <see cref="View.SetOnTouchListener"/> (replacing) so that MAUI's internal gesture
    /// handling (tap, long-press, etc.) is not disrupted.
    /// </remarks>
    internal sealed class DragThresholdTouchHandler
    {
        /// <summary>
        /// Stores the currently dragged item view model so that drop handlers can retrieve it.
        /// This is set when a drag starts and cleared when the drag ends.
        /// </summary>
        internal static BrowserItemViewModel? CurrentDraggedItem { get; set; }

        /// <summary>
        /// The MIME type used for internal drag-and-drop operations.
        /// </summary>
        internal const string DragMimeType = "application/x-sffs-drag-item";

        private readonly Microsoft.Maui.Controls.View _mauiView;
        private float _startX;
        private float _startY;
        private int _touchSlop;
        private bool _isDragStarted;
        private bool _isTracking;

        public DragThresholdTouchHandler(Microsoft.Maui.Controls.View mauiView)
        {
            _mauiView = mauiView;
        }

        /// <summary>
        /// Attaches to the given Android view's <see cref="View.Touch"/> event.
        /// </summary>
        public void Attach(View androidView)
        {
            androidView.Touch += OnTouch;
        }

        private void OnTouch(object? sender, View.TouchEventArgs e)
        {
            var v = sender as View;
            var motionEvent = e.Event;

            if (v is null || motionEvent is null)
            {
                e.Handled = false;
                return;
            }

            switch (motionEvent.ActionMasked)
            {
                case MotionEventActions.Down:
                    _startX = motionEvent.RawX;
                    _startY = motionEvent.RawY;
                    _isDragStarted = false;
                    _isTracking = true;
                    _touchSlop = v.Context is not null ? ViewConfiguration.Get(v.Context)?.ScaledTouchSlop ?? 24 : 24;
                    e.Handled = false;
                    break;

                case MotionEventActions.Move:
                    if (!_isTracking || _isDragStarted)
                    {
                        e.Handled = false;
                        break;
                    }

                    var deltaX = motionEvent.RawX - _startX;
                    var deltaY = motionEvent.RawY - _startY;
                    var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                    // Only start drag after exceeding the touch slop threshold
                    if (distance > _touchSlop)
                    {
                        _isDragStarted = true;
                        _isTracking = false;
                        StartNativeDrag(v);
                        e.Handled = true; // Consume the event to prevent further touch processing
                    }
                    else
                    {
                        e.Handled = false;
                    }
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    _isDragStarted = false;
                    _isTracking = false;
                    e.Handled = false;
                    break;

                default:
                    e.Handled = false;
                    break;
            }
        }

        /// <summary>
        /// Starts a native Android drag-and-drop operation, bypassing MAUI's <see cref="DragGestureRecognizer"/>
        /// entirely. The dragged item is stored in <see cref="CurrentDraggedItem"/> so that
        /// MAUI drop handlers can retrieve it.
        /// </summary>
        private void StartNativeDrag(View androidView)
        {
            if (_mauiView.BindingContext is not BrowserItemViewModel itemViewModel)
                return;

            // Store the dragged item so drop handlers can access it
            CurrentDraggedItem = itemViewModel;

            // Create a ClipData with our custom MIME type
            var clipData = ClipData.NewPlainText(DragMimeType, itemViewModel.Inner.Id);

            // Create a drag shadow from the view
            var shadowBuilder = new View.DragShadowBuilder(androidView);

            // Start the native drag
            androidView.StartDragAndDrop(clipData, shadowBuilder, null, (int)DragFlags.Global);
        }
    }
}

