using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Maui.AppModels;

namespace SecureFolderFS.Maui.UserControls.Common
{
    public sealed partial class GalleryView : ContentView, IDisposable
    {
        private enum GestureLockAxis
        {
            None,
            Horizontal,
            Vertical
        }

        private readonly CorrectedPanGestureRecognizer _recognizer;
        private readonly Grid _container;
        private double _translateX;

        // Dismiss drag state
        private double _dismissTranslateY;
        private GestureLockAxis _lockedAxis = GestureLockAxis.None;
        private const double AxisLockThresholdPx = 8d;

        private View? _previousView;
        private View? _currentView;
        private View? _nextView;

        public event EventHandler? NextRequested;
        public event EventHandler? PreviousRequested;
        public event EventHandler? DismissRequested;

        public View? Previous
        {
            get => _previousView;
            set => _previousView = value;
        }

        public View? Current
        {
            get => _currentView;
            set => _currentView = value;
        }

        public View? Next
        {
            get => _nextView;
            set => _nextView = value;
        }

        public GalleryView()
        {
            _container = new Grid()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition()
                },
                IsClippedToBounds = true
            };
            Content = _container;
            PanUpdatedCommand = new RelayCommand<PanUpdatedEventArgs>(args =>
                Recognizer_PanUpdated(this, args ?? throw new ArgumentNullException(nameof(args))));

            _recognizer = new CorrectedPanGestureRecognizer();
            _recognizer.PanUpdated += Recognizer_PanUpdated;
            GestureRecognizers.Add(_recognizer);
        }

        public void RefreshLayout()
        {
            _container.Children.Clear();

            if (_previousView != null)
            {
                _container.Children.Add(_previousView);
                Grid.SetColumn(_previousView, 0);
            }

            if (_currentView != null)
            {
                _container.Children.Add(_currentView);
                Grid.SetColumn(_currentView, 1);
            }

            if (_nextView != null)
            {
                _container.Children.Add(_nextView);
                Grid.SetColumn(_nextView, 2);
            }

            // Reset position to center
            _container.TranslationX = 0;
        }

        public void FitToParent(ContentView? parent = null)
        {
            parent ??= Parent as ContentView;
            var height = parent?.Height ?? 300d;
            var width = parent?.Width ?? 400d;

            WidthRequest = width;
            HeightRequest = height;
        }

        public async Task SwipeToPreviousAsync()
        {
            await _container.TranslateToAsync(Width, 0, 225U, Easing.CubicOut);

            // Shift views
            var oldCurrent = _currentView;
            _currentView = _previousView;

            (_nextView as IDisposable)?.Dispose();

            _nextView = oldCurrent;
            _previousView = null;

            PreviousRequested?.Invoke(this, EventArgs.Empty);
            RefreshLayout();
        }

        public async Task SwipeToNextAsync()
        {
            await _container.TranslateToAsync(-Width, 0, 225U, Easing.CubicOut);

            // Shift views
            var oldCurrent = _currentView;
            _currentView = _nextView;

            (_previousView as IDisposable)?.Dispose();

            _previousView = oldCurrent;
            _nextView = null;

            NextRequested?.Invoke(this, EventArgs.Empty);
            RefreshLayout();
        }

        /// <inheritdoc/>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            _container.WidthRequest = width * 3;
            _container.TranslationX = 0;
        }

        private async Task ResetPositionAsync()
        {
            await _container.TranslateToAsync(0, 0, 225U, Easing.CubicOut);
        }

        private async Task ResetDismissDragAsync()
        {
            var translateTask = this.TranslateToAsync(0, 0, 200U, Easing.CubicOut);
            var fadeTask = this.FadeToAsync(1d, 200U, Easing.CubicOut);
            await Task.WhenAll(translateTask, fadeTask);
            _dismissTranslateY = 0d;
        }

        private async void Recognizer_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                {
                    _lockedAxis = GestureLockAxis.None;
                    _translateX = 0d;
                    _dismissTranslateY = 0d;
                    break;
                }

                case GestureStatus.Running:
                {
                    // Determine axis lock from first significant movement
                    if (_lockedAxis == GestureLockAxis.None)
                    {
                        var absX = Math.Abs(e.TotalX);
                        var absY = Math.Abs(e.TotalY);

                        if (absX < AxisLockThresholdPx && absY < AxisLockThresholdPx)
                            break; // Not enough movement to decide yet

                        _lockedAxis = absY > absX ? GestureLockAxis.Vertical : GestureLockAxis.Horizontal;
                    }

                    if (_lockedAxis == GestureLockAxis.Vertical)
                    {
                        if (!IsDismissible || e.TotalY <= 0d)
                            break; // Only allow downward drag for dismiss

                        _dismissTranslateY = e.TotalY;

                        // Translate the whole gallery downward
                        TranslationY = _dismissTranslateY;

                        // Fade out subtly as user drags — reaches ~0.4 opacity at the dismiss threshold
                        var dismissThreshold = Height / 5d;
                        var progress = Math.Clamp(_dismissTranslateY / dismissThreshold, 0d, 1d);
                        Opacity = 1d - (progress * 0.6d);
                    }
                    else // Horizontal
                    {
                        _translateX = e.TotalX;
                        _container.TranslationX = _translateX;
                    }

                    break;
                }

                case GestureStatus.Canceled:
                {
                    if (_lockedAxis == GestureLockAxis.Vertical)
                        await ResetDismissDragAsync();
                    else
                        await ResetPositionAsync();

                    _lockedAxis = GestureLockAxis.None;
                    break;
                }

                case GestureStatus.Completed:
                {
                    if (_lockedAxis == GestureLockAxis.Vertical)
                    {
                        var dismissThreshold = Height / 5d;
                        if (IsDismissible && _dismissTranslateY > dismissThreshold)
                            DismissRequested?.Invoke(this, EventArgs.Empty);
                        else
                            await ResetDismissDragAsync();
                    }
                    else // Horizontal
                    {
                        var threshold = Width / 6;

                        if (_translateX > threshold && _previousView != null)
                            await SwipeToPreviousAsync();
                        else if (_translateX < -threshold && _nextView != null)
                            await SwipeToNextAsync();
                        else
                            await ResetPositionAsync();
                    }

                    _lockedAxis = GestureLockAxis.None;
                    break;
                }
            }
        }

        public ICommand? PanUpdatedCommand
        {
            get => (ICommand?)GetValue(PanUpdatedCommandProperty);
            set => SetValue(PanUpdatedCommandProperty, value);
        }
        public static readonly BindableProperty PanUpdatedCommandProperty =
            BindableProperty.Create(nameof(PanUpdatedCommand), typeof(ICommand), typeof(GalleryView));

        public bool IsDismissible
        {
            get => (bool)GetValue(IsDismissibleProperty);
            set => SetValue(IsDismissibleProperty, value);
        }
        public static readonly BindableProperty IsDismissibleProperty =
            BindableProperty.Create(nameof(IsDismissible), typeof(bool), typeof(GalleryView), false);

        /// <inheritdoc/>
        public void Dispose()
        {
            NextRequested = null;
            PreviousRequested = null;
            DismissRequested = null;
            PanUpdatedCommand = null;
            _recognizer.PanUpdated -= Recognizer_PanUpdated;
        }
    }
}
