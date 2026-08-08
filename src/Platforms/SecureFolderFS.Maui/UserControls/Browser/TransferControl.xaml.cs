using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class TransferControl : ContentView
    {
        private const double BOUNCE_LIMIT = -16d;
        private const double HIDE_TRANSLATION = 200d;
        private const double DISMISS_THRESHOLD = 40d;
        private const double EXPANDED_DISMISS_THRESHOLD = 120d;
        private const double COLLAPSED_HEIGHT = 56d;
        private const double COLLAPSED_INSET = 16d;
        private const double COLLAPSED_BOTTOM_INSET = 32d;
        private const double COLLAPSED_CORNER_RADIUS = 12d;
        private const double EXPANDED_CORNER_RADIUS = 20d;
        private const double EXPANDED_TOP_INSET = 24d;
        private const uint EXPAND_DURATION = 300U;

        private bool _isDismissing;
        private bool _isExpanded;
        private bool _isExpanding;
        private double _expandedHeight;

        public TransferControl()
        {
            InitializeComponent();
        }

        #region Dragging

        private async void Panel_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            // The banner is dragged as a whole, but once expanded only the header is a grab area
            if (_isExpanded)
                return;

            await HandlePanAsync(e);
        }

        private async void Header_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (!_isExpanded)
                return;

            await HandlePanAsync(e);
        }

        private async Task HandlePanAsync(PanUpdatedEventArgs e)
        {
            try
            {
                if (_isDismissing || _isExpanding)
                    return;

                // Swipe-down dismiss cancels the operation. Disallow it for non-cancellable operations
                if (!CanCancel)
                {
                    if (Surface.TranslationY != 0d)
                        await Surface.TranslateToAsync(0, 0, 250U, Easing.SpringOut);

                    return;
                }

                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                    {
                        // A snap-back from a previous drag may still be in flight, and it would
                        // write TranslationY behind the finger's back for the rest of this gesture
                        Surface.CancelAnimations();
                        break;
                    }

                    case GestureStatus.Running:
                    {
                        var translation = e.TotalY;
                        Surface.TranslationY = translation < 0
                            ? Math.Max(translation / 4d, BOUNCE_LIMIT)
                            : translation;
                        break;
                    }

                    case GestureStatus.Completed:
                    case GestureStatus.Canceled:
                    {
                        // A full-height sheet needs a longer pull than a banner before it reads as a dismissal
                        var threshold = _isExpanded ? EXPANDED_DISMISS_THRESHOLD : DISMISS_THRESHOLD;
                        if (Surface.TranslationY >= threshold)
                        {
                            _isDismissing = true;
                            try
                            {
                                // Animate out from the current dragged position
                                await AnimateOutAsync(300d);
                            }
                            finally
                            {
                                // Always reset, otherwise a failed animation would wedge the control
                                _isDismissing = false;
                            }

                            // Tell the caller - they will set IsShown=false, which the guard will skip animating.
                            // This also ensures the backing value is actually false, so the next IsShown=true fires propertyChanged.
                            CancelCommand?.Execute(null);
                        }
                        else
                        {
                            await Surface.TranslateToAsync(0, 0, 250U, Easing.SpringOut);
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task AnimateOutAsync(double baseDuration)
        {
            // An expanded sheet reaches far above the banner, so it has more distance to cover to clear the screen
            var restingDistance = _isExpanded
                ? Math.Max(RootPanel.Height, HIDE_TRANSLATION)
                : HIDE_TRANSLATION;

            // A hard drag can fling the surface past its exit point. Carrying on from wherever the
            // finger left it beats yanking it back up to the exit point before it leaves
            var currentY = Surface.TranslationY;
            var hideTranslation = Math.Max(restingDistance, currentY);
            var remainingDistance = hideTranslation - currentY;
            if (remainingDistance > 0.5d)
            {
                var duration = (uint)Math.Max(baseDuration * (remainingDistance / restingDistance), 150d);
                await Surface.TranslateToAsync(0, hideTranslation, duration, Easing.CubicInOut);
            }

            // Clean up visual state, so the next reveal starts out as a collapsed banner again
            RootPanel.IsVisible = false;
            Surface.TranslationY = 0d;
            ResetExpansion();
        }

        #endregion

        #region Expanding

        private async void Surface_Tapped(object? sender, TappedEventArgs e)
        {
            try
            {
                if (_isExpanded || _isExpanding || _isDismissing)
                    return;

                if (!IsError || string.IsNullOrWhiteSpace(ErrorDetails))
                    return;

                // The banner dismisses itself on a timer, which must not happen while the report is being read
                HoldCommand?.Execute(null);
                await ExpandAsync();
            }
            catch (Exception)
            {
            }
        }

        private async Task ExpandAsync()
        {
            _isExpanding = true;
            try
            {
                _expandedHeight = Math.Max(GetAvailableHeight() - EXPANDED_TOP_INSET, COLLAPSED_HEIGHT);
                _isExpanded = true;

                // A pan anywhere on the panel would fight the report's ScrollView from here on,
                // so the grab strip over the header takes over as the only drag handle
                RootPanel.GestureRecognizers.Remove(SurfacePan);
                DetailsGrabStrip.IsVisible = true;
                BannerContent.InputTransparent = true;
                DetailsContent.Opacity = 0d;
                DetailsContent.IsVisible = true;

                await AnimateAsync("TransferExpansion", EXPAND_DURATION, Easing.CubicInOut, t =>
                {
                    ApplyExpansion(t);

                    // The banner clears out early, so the two states barely overlap during the cross-fade
                    BannerContent.Opacity = Math.Clamp(1d - t * 2.5d, 0d, 1d);
                    DetailsContent.Opacity = Math.Clamp((t - 0.35d) / 0.65d, 0d, 1d);
                });

                ApplyExpansion(1d);
                BannerContent.Opacity = 0d;
                BannerContent.IsVisible = false;
                DetailsContent.Opacity = 1d;
            }
            finally
            {
                _isExpanding = false;
            }
        }

        /// <summary>
        /// Applies the collapsed (0) to expanded (1) layout, where the control grows upwards
        /// out of the banner and flushes itself against the bottom and side edges.
        /// </summary>
        private void ApplyExpansion(double progress)
        {
            var inverse = 1d - progress;
            var topRadius = COLLAPSED_CORNER_RADIUS + (EXPANDED_CORNER_RADIUS - COLLAPSED_CORNER_RADIUS) * progress;
            var bottomRadius = COLLAPSED_CORNER_RADIUS * inverse;

            Surface.HeightRequest = COLLAPSED_HEIGHT + (_expandedHeight - COLLAPSED_HEIGHT) * progress;
            Surface.Margin = new Thickness(COLLAPSED_INSET * inverse);
            SurfaceShape.CornerRadius = new CornerRadius(topRadius, topRadius, bottomRadius, bottomRadius);
            RootPanel.Margin = new Thickness(0d, 0d, 0d, COLLAPSED_BOTTOM_INSET * inverse);
        }

        /// <summary>
        /// Restores the collapsed banner without animating. Used once the control is off-screen.
        /// </summary>
        private void ResetExpansion()
        {
            _isExpanded = false;
            _expandedHeight = COLLAPSED_HEIGHT;
            ApplyExpansion(0d);

            BannerContent.Opacity = 1d;
            BannerContent.IsVisible = true;
            BannerContent.InputTransparent = false;
            DetailsContent.Opacity = 0d;
            DetailsContent.IsVisible = false;
            DetailsGrabStrip.IsVisible = false;

            if (!RootPanel.GestureRecognizers.Contains(SurfacePan))
                RootPanel.GestureRecognizers.Add(SurfacePan);
        }

        /// <summary>
        /// Gets the height the expanded sheet may grow into. The control itself is only as tall as
        /// the banner, so the space is measured against whatever hosts it.
        /// </summary>
        private double GetAvailableHeight()
        {
            var available = (Parent as VisualElement)?.Height ?? 0d;
            if (available <= 0d)
                available = Window?.Height ?? 0d;

            return available;
        }

        private Task AnimateAsync(string name, uint length, Easing easing, Action<double> step)
        {
            // Height and margin have no built-in *ToAsync counterpart, so the interpolation is driven by hand
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            new Animation(step).Commit(this, name, 16U, length, easing, (_, _) => tcs.TrySetResult());

            return tcs.Task;
        }

        #endregion

        public bool IsShown
        {
            get => (bool)GetValue(IsShownProperty);
            set => SetValue(IsShownProperty, value);
        }
        public static readonly BindableProperty IsShownProperty =
            BindableProperty.Create(nameof(IsShown), typeof(bool), typeof(TransferControl), false,
                propertyChanged: static async (bindable, _, newValue) =>
                {
                    if (newValue is not bool bValue || bindable is not TransferControl tc)
                        return;

                    try
                    {
                        if (tc._isDismissing)
                        {
                            // Gesture already handled the animation; just ensure a clean state
                            tc.RootPanel.IsVisible = false;
                            tc.Surface.TranslationY = 0d;
                            tc.ResetExpansion();
                            return;
                        }

                        if (bValue)
                        {
                            // Whatever was left expanded belongs to the previous operation
                            tc.ResetExpansion();
                            tc.Surface.TranslationY = HIDE_TRANSLATION;
                            tc.RootPanel.IsVisible = true;
                            await tc.Surface.TranslateToAsync(0, 0, 350U, Easing.CubicInOut);
                        }
                        else
                        {
                            await tc.AnimateOutAsync(350d);
                        }
                    }
                    catch (Exception)
                    {
                        // An async void handler must never let an animation failure reach the app
                    }
                });

        public bool CanCancel
        {
            get => (bool)GetValue(CanCancelProperty);
            set => SetValue(CanCancelProperty, value);
        }
        public static readonly BindableProperty CanCancelProperty =
            BindableProperty.Create(nameof(CanCancel), typeof(bool), typeof(TransferControl), true);

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(TransferControl), false);

        public bool IsConfirmShown
        {
            get => (bool)GetValue(IsConfirmShownProperty);
            set => SetValue(IsConfirmShownProperty, value);
        }
        public static readonly BindableProperty IsConfirmShownProperty =
            BindableProperty.Create(nameof(IsConfirmShown), typeof(bool), typeof(TransferControl), false);

        public bool IsError
        {
            get => (bool)GetValue(IsErrorProperty);
            set => SetValue(IsErrorProperty, value);
        }
        public static readonly BindableProperty IsErrorProperty =
            BindableProperty.Create(nameof(IsError), typeof(bool), typeof(TransferControl), false);

        public bool IsSuccess
        {
            get => (bool)GetValue(IsSuccessProperty);
            set => SetValue(IsSuccessProperty, value);
        }
        public static readonly BindableProperty IsSuccessProperty =
            BindableProperty.Create(nameof(IsSuccess), typeof(bool), typeof(TransferControl), false);

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(TransferControl));

        public string? ErrorDetails
        {
            get => (string?)GetValue(ErrorDetailsProperty);
            set => SetValue(ErrorDetailsProperty, value);
        }
        public static readonly BindableProperty ErrorDetailsProperty =
            BindableProperty.Create(nameof(ErrorDetails), typeof(string), typeof(TransferControl));

        public string? PrimaryButtonText
        {
            get => (string?)GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }
        public static readonly BindableProperty PrimaryButtonTextProperty =
            BindableProperty.Create(nameof(PrimaryButtonText), typeof(string), typeof(TransferControl));

        public ICommand? CancelCommand
        {
            get => (ICommand?)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly BindableProperty CancelCommandProperty =
            BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(TransferControl));

        public ICommand? PrimaryCommand
        {
            get => (ICommand?)GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }
        public static readonly BindableProperty PrimaryCommandProperty =
            BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(TransferControl));

        public ICommand? HoldCommand
        {
            get => (ICommand?)GetValue(HoldCommandProperty);
            set => SetValue(HoldCommandProperty, value);
        }
        public static readonly BindableProperty HoldCommandProperty =
            BindableProperty.Create(nameof(HoldCommand), typeof(ICommand), typeof(TransferControl));
    }
}
