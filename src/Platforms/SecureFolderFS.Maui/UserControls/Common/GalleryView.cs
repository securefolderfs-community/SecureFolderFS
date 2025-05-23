using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace SecureFolderFS.Maui.UserControls.Common
{
    public sealed partial class GalleryView : ContentView, IDisposable
    {
        private readonly PanGestureRecognizer _recognizer;
        private readonly Grid _container;
        private double _translateX;

        private View? _previousView;
        private View? _currentView;
        private View? _nextView;

        public event EventHandler? NextRequested;
        public event EventHandler? PreviousRequested;

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
        
        public ICommand? PanUpdatedCommand { get; private set; }

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
            PanUpdatedCommand = new RelayCommand<PanUpdatedEventArgs>(args => Recognizer_PanUpdated(this, args ?? throw new ArgumentNullException(nameof(args))));

            _recognizer = new();
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

        private async void Recognizer_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                {
                    _translateX = e.TotalX;
                    _container.TranslationX = _translateX;
                    break;
                }

                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                {
                    var threshold = Width / 4;

                    if (_translateX > threshold && _previousView != null)
                        await SwipeToPreviousAsync();
                    else if (_translateX < -threshold && _nextView != null)
                        await SwipeToNextAsync();
                    else
                        await ResetPositionAsync();

                    break;
                }
            }
        }

        private async Task SwipeToPreviousAsync()
        {
            await _container.TranslateTo(Width, 0, 225U, Easing.CubicOut);

            // Shift views
            var oldCurrent = _currentView;
            _currentView = _previousView;
            
            (_nextView as IDisposable)?.Dispose();
            
            _nextView = oldCurrent;
            _previousView = null;

            PreviousRequested?.Invoke(this, EventArgs.Empty);
            RefreshLayout();
        }

        private async Task SwipeToNextAsync()
        {
            await _container.TranslateTo(-Width, 0, 225U, Easing.CubicOut);

            // Shift views
            var oldCurrent = _currentView;
            _currentView = _nextView;

            (_previousView as IDisposable)?.Dispose();
            
            _previousView = oldCurrent;
            _nextView = null;

            NextRequested?.Invoke(this, EventArgs.Empty);
            RefreshLayout();
        }

        private async Task ResetPositionAsync()
        {
            await _container.TranslateTo(0, 0, 225U, Easing.CubicOut);
        }

        /// <inheritdoc/>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            _container.WidthRequest = width * 3;
            _container.TranslationX = 0;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            NextRequested = null;
            PreviousRequested = null;
            PanUpdatedCommand = null;
            _recognizer.PanUpdated -= Recognizer_PanUpdated;
        }
    }
}
