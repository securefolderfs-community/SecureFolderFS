namespace SecureFolderFS.Maui.UserControls
{
    /// <summary>
    /// Specifies which side the reveal content appears on and implicitly defines the orientation axis.
    /// Left/Right = horizontal axis. Top/Bottom = vertical axis.
    /// </summary>
    public enum RevealSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// A layout that animates a piece of <see cref="RevealContent"/> in and out, sliding the
    /// <see cref="MainContent"/> along the same axis to fill or vacate the space.
    /// </summary>
    public sealed class AnimatedRevealLayout : Grid
    {
        private const uint ANIMATION_DURATION = 250U;
        private static readonly Easing ShowEasing = Easing.CubicOut;
        private static readonly Easing HideEasing = Easing.CubicIn;

        // Wraps the reveal content and is the thing whose Width/Height we animate
        private readonly ContentView _revealWrapper = new()
        {
            Padding = 0,
            IsClippedToBounds = true
        };

        private View? _revealView;
        private View? _mainView;
        private double _naturalRevealSize = -1d;
        private bool _isAnimating;

        public AnimatedRevealLayout()
        {
            // Wrapper starts collapsed if IsShown defaults to false
            _revealWrapper.IsVisible = true;
        }

        #region Content change handlers

        private void OnRevealContentChanged(View? oldView, View? newView)
        {
            _revealWrapper.Content = newView;
            _revealView = newView;
            _naturalRevealSize = -1d;
            RebuildLayout();
        }

        private void OnMainContentChanged(View? oldView, View? newView)
        {
            if (oldView is not null && Children.Contains(oldView))
                Children.Remove(oldView);

            _mainView = newView;
            RebuildLayout();
        }

        #endregion

        #region Layout building

        private bool IsHorizontal => RevealSide is RevealSide.Left or RevealSide.Right;

        private bool IsRevealAtStart => RevealSide is RevealSide.Left or RevealSide.Top;

        private void RebuildLayout()
        {
            Children.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            if (IsHorizontal)
            {
                ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // reveal slot
                ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // main slot
                ColumnSpacing = Spacing;
                RowSpacing = 0;

                var (revealCol, mainCol) = IsRevealAtStart ? (0, 1) : (1, 0);
                Grid.SetColumn(_revealWrapper, revealCol);

                // Collapse wrapper width to 0 when hidden; natural size when shown
                SetWrapperSize(IsShown ? GetNaturalSize() : 0d);
                _revealWrapper.Opacity = IsShown ? 1d : 0d;
                Children.Add(_revealWrapper);

                if (_mainView is not null)
                {
                    Grid.SetColumn(_mainView, mainCol);
                    _mainView.VerticalOptions = LayoutOptions.Center;
                    Children.Add(_mainView);
                }
            }
            else
            {
                RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                RowSpacing = Spacing;
                ColumnSpacing = 0;

                var (revealRow, mainRow) = IsRevealAtStart ? (0, 1) : (1, 0);
                Grid.SetRow(_revealWrapper, revealRow);
                SetWrapperSize(IsShown ? GetNaturalSize() : 0d);
                _revealWrapper.Opacity = IsShown ? 1d : 0d;
                Children.Add(_revealWrapper);

                if (_mainView is not null)
                {
                    Grid.SetRow(_mainView, mainRow);
                    _mainView.HorizontalOptions = LayoutOptions.Center;
                    Children.Add(_mainView);
                }
            }
        }

        private void SetWrapperSize(double size)
        {
            if (IsHorizontal)
            {
                _revealWrapper.WidthRequest = size;
                _revealWrapper.HeightRequest = -1;
            }
            else
            {
                _revealWrapper.HeightRequest = size;
                _revealWrapper.WidthRequest = -1;
            }
        }

        #endregion

        #region Animation

        private async Task AnimateRevealAsync(bool show)
        {
            // Resolve natural size before animating
            if (_naturalRevealSize <= 0d)
                _naturalRevealSize = GetNaturalSize();

            if (_naturalRevealSize <= 0d)
            {
                await Task.Delay(16);
                _naturalRevealSize = GetNaturalSize();
            }

            _isAnimating = true;
            var fromSize = IsHorizontal ? _revealWrapper.WidthRequest : _revealWrapper.HeightRequest;
            var toSize = show ? _naturalRevealSize : 0d;
            var fromOpacity = _revealWrapper.Opacity;
            var toOpacity = show ? 1d : 0d;
            var easing = show ? ShowEasing : HideEasing;
            var animation = new Animation(t =>
            {
                var size = fromSize + (toSize - fromSize) * t;
                var opacity = fromOpacity + (toOpacity - fromOpacity) * t;
                SetWrapperSize(size);
                _revealWrapper.Opacity = opacity;
            }, 0d, 1d, easing);

            animation.Commit(this, "RevealAnimation", length: ANIMATION_DURATION);

            // Await by waiting for the animation to finish
            await Task.Delay((int)ANIMATION_DURATION + 16);

            // Snap to exact final values to avoid floating point drift
            SetWrapperSize(toSize);
            _revealWrapper.Opacity = toOpacity;

            _isAnimating = false;
        }

        private double GetNaturalSize()
        {
            if (_revealView is null)
                return 0d;

            return IsHorizontal
                ? _revealView.WidthRequest > 0d ? _revealView.WidthRequest : _revealView.Width
                : _revealView.HeightRequest > 0d
                    ? _revealView.HeightRequest
                    : _revealView.Height;
        }

        #endregion

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (_naturalRevealSize <= 0d)
            {
                _naturalRevealSize = GetNaturalSize();

                // Correct wrapper size now that layout has run
                if (!_isAnimating)
                    SetWrapperSize(IsShown ? _naturalRevealSize : 0d);
            }
        }

        #region Bindable Properties

        public View? RevealContent
        {
            get => (View?)GetValue(RevealContentProperty);
            set => SetValue(RevealContentProperty, value);
        }

        public static readonly BindableProperty RevealContentProperty =
            BindableProperty.Create(nameof(RevealContent), typeof(View), typeof(AnimatedRevealLayout),
                propertyChanged: static (bindable, oldValue, newValue) =>
                {
                    if (bindable is AnimatedRevealLayout layout)
                        layout.OnRevealContentChanged(oldValue as View, newValue as View);
                });

        public View? MainContent
        {
            get => (View?)GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }

        public static readonly BindableProperty MainContentProperty =
            BindableProperty.Create(nameof(MainContent), typeof(View), typeof(AnimatedRevealLayout),
                propertyChanged: static (bindable, oldValue, newValue) =>
                {
                    if (bindable is AnimatedRevealLayout layout)
                        layout.OnMainContentChanged(oldValue as View, newValue as View);
                });

        public bool IsShown
        {
            get => (bool)GetValue(IsShownProperty);
            set => SetValue(IsShownProperty, value);
        }

        public static readonly BindableProperty IsShownProperty =
            BindableProperty.Create(nameof(IsShown), typeof(bool), typeof(AnimatedRevealLayout), false,
                propertyChanged: static (bindable, _, newValue) =>
                {
                    if (bindable is AnimatedRevealLayout layout && newValue is bool bValue)
                        _ = layout.AnimateRevealAsync(bValue);
                });

        public RevealSide RevealSide
        {
            get => (RevealSide)GetValue(RevealSideProperty);
            set => SetValue(RevealSideProperty, value);
        }

        public static readonly BindableProperty RevealSideProperty =
            BindableProperty.Create(nameof(RevealSide), typeof(RevealSide), typeof(AnimatedRevealLayout),
                RevealSide.Left,
                propertyChanged: static (bindable, _, _) =>
                {
                    if (bindable is AnimatedRevealLayout layout)
                        layout.RebuildLayout();
                });

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create(nameof(Spacing), typeof(double), typeof(AnimatedRevealLayout), 0d,
                propertyChanged: static (bindable, _, _) =>
                {
                    if (bindable is AnimatedRevealLayout layout)
                        layout.RebuildLayout();
                });

        #endregion
    }
}
