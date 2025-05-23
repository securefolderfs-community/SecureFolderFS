// Some parts of the following code were used from MauiPanPinchContainer on the MIT License basis.
// See the associated license file for more information.

using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Maui.AppModels;

namespace SecureFolderFS.Maui.UserControls.Common
{
    /// <summary>
    /// <para><see href="https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/pan"/></para>
    /// <para><see href="https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/gestures/pinch"/></para>
    /// </summary>
    public class PanPinchContainer : ContentView
    {
        private readonly TapGestureRecognizer _doubleTapGestureRecognizer;
        private readonly PinchGestureRecognizer _pinchGestureRecognizer;
        private readonly CorrectedPanGestureRecognizer _panGestureRecognizer;
        private bool _isPanEnabled = true;
        private double _startScale = 1;
        private double _panX;
        private double _panY;
        
        public double CurrentScale { get; private set; } = 1;
        
        public ICommand? PanUpdatedCommand { get; set; }
        
        public PanPinchContainer()
        {
            _panGestureRecognizer = new CorrectedPanGestureRecognizer();
            _pinchGestureRecognizer = new PinchGestureRecognizer();
            _doubleTapGestureRecognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };

            _panGestureRecognizer.PanUpdated += GestureRecognizer_PanUpdated;
            _pinchGestureRecognizer.PinchUpdated += GestureRecognizer_PinchUpdated;
            _doubleTapGestureRecognizer.Tapped += Recognizer_DoubleTapped;

            GestureRecognizers.Add(_panGestureRecognizer);
            GestureRecognizers.Add(_pinchGestureRecognizer);
            GestureRecognizers.Add(_doubleTapGestureRecognizer);
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);
            if (child is not View view)
                return;

            view.HorizontalOptions = LayoutOptions.Center;
            view.VerticalOptions = LayoutOptions.Center;
        }

        private async Task ClampTranslationAsync(double transX, double transY, bool animate = false)
        {
            Content.AnchorX = 0;
            Content.AnchorY = 0;

            var contentWidth = Content.Width * CurrentScale;
            var contentHeight = Content.Height * CurrentScale;

            if (contentWidth <= Width)
            {
                transX = -(contentWidth - Content.Width) / 2;
            }
            else
            {
                var minBoundX = ((Width - Content.Width) / 2) + contentWidth - Width;
                var maxBoundX = (Width - Content.Width) / 2;
                transX = Math.Clamp(transX, -minBoundX, -maxBoundX);
            }

            if (contentHeight <= Height)
                transY = -(contentHeight - Content.Height) / 2;
            else
            {
                var minBoundY = ((Height - Content.Height) / 2) + contentHeight - Height;
                var maxBoundY = (Height - Content.Height) / 2;
                transY = Math.Clamp(transY, -minBoundY, -maxBoundY);
            }

            if (animate)
                await TranslateToAsync(transX, transY);
            else
            {
                Content.TranslationX = transX;
                Content.TranslationY = transY;
            }
        }

        private async Task ClampTranslationFromScaleOriginAsync(double originX, double originY, bool animate = false)
        {
            // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
            // so get the X pixel coordinate.
            double renderedX = Content.X + _panX;
            double deltaX = renderedX / Width;
            double deltaWidth = Width / (Content.Width * _startScale);
            originX = (originX - deltaX) * deltaWidth;

            // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
            // so get the Y pixel coordinate.
            double renderedY = Content.Y + _panY;
            double deltaY = renderedY / Height;
            double deltaHeight = Height / (Content.Height * _startScale);
            originY = (originY - deltaY) * deltaHeight;

            // Calculate the transformed element pixel coordinates.
            double targetX = _panX - (originX * Content.Width * (CurrentScale - _startScale));
            double targetY = _panY - (originY * Content.Height * (CurrentScale - _startScale));

            // Apply translation based on the change in origin.
            if (CurrentScale > 1)
            {
                targetX = Math.Clamp(targetX, -Content.Width * (CurrentScale - 1), 0);
                targetY = Math.Clamp(targetY, -Content.Height * (CurrentScale - 1), 0);
            }
            else
            {
                targetX = (Width - (Content.Width * CurrentScale)) / 2;
                targetY = Content.Height * (1 - CurrentScale) / 2;
            }

            await ClampTranslationAsync(targetX, targetY, animate);
        }

        private async void Recognizer_DoubleTapped(object? sender, TappedEventArgs e)
        {
            _startScale = Content.Scale;
            CurrentScale = _startScale;
            _panX = Content.TranslationX;
            _panY = Content.TranslationY;
            CurrentScale = CurrentScale < 2 ? 2 : 1;

            var point = e.GetPosition(sender as View);
            var translateTask = Task.CompletedTask;
            if (point is not null)
                translateTask = ClampTranslationFromScaleOriginAsync(point.Value.X / Width, point.Value.Y / Height, true);

            var scaleTask = ScaleToAsync(CurrentScale);
            await Task.WhenAll(translateTask, scaleTask);
            _panX = Content.TranslationX;
            _panY = Content.TranslationY;
        }

        private async void GestureRecognizer_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (!_isPanEnabled)
                return;

            if (CurrentScale < 1.1d)
            {
                PanUpdatedCommand?.Execute(e);
                return;
            }

            if (Content.Scale <= 1)
                return;

            if (e.StatusType == GestureStatus.Started)
            {
                _panX = Content.TranslationX;
                _panY = Content.TranslationY;

                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                // Translate and pan.
                await ClampTranslationAsync(_panX + e.TotalX, _panY + e.TotalY);
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                // Store the translation applied during the pan
                _panX = Content.TranslationX;
                _panY = Content.TranslationY;
            }
            else if (e.StatusType == GestureStatus.Canceled)
            {
                Content.TranslationX = _panX;
                Content.TranslationY = _panX;
            }
        }

        private async void GestureRecognizer_PinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                _isPanEnabled = false;

                _panX = Content.TranslationX;
                _panY = Content.TranslationY;

                // Store the current scale factor applied to the wrapped user interface element,
                // and zero the components for the center point of the translate transform.
                _startScale = Content.Scale;

                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }

            if (e.Status == GestureStatus.Running)
            {
                // Calculate the scale factor to be applied.
                CurrentScale += (e.Scale - 1) * _startScale;
                CurrentScale = Math.Clamp(CurrentScale, 0.5, 10);

                await ClampTranslationFromScaleOriginAsync(e.ScaleOrigin.X, e.ScaleOrigin.Y);

                // Apply scale factor
                Content.Scale = CurrentScale;
            }

            if (e.Status == GestureStatus.Completed)
            {
                if (CurrentScale < 1)
                {
                    var translateTask = TranslateToAsync(0, 0);
                    var scaleTask = ScaleToAsync(1);

                    await Task.WhenAll(translateTask, scaleTask);
                }

                // Store the translation delta's of the wrapped user interface element.
                _panX = Content.TranslationX;
                _panY = Content.TranslationY;

                _isPanEnabled = true;
            }
            else if (e.Status == GestureStatus.Canceled)
            {
                Content.TranslationX = _panX;
                Content.TranslationY = _panY;
                Content.Scale = _startScale;

                _isPanEnabled = true;
            }
        }

        private async Task ScaleToAsync(double scale)
        {
            await Content.ScaleTo(scale, 250, Easing.Linear);
            CurrentScale = scale;
        }

        private async Task TranslateToAsync(double x, double y)
        {
            await Content.TranslateTo(x, y, 250, Easing.Linear);
            _panX = x;
            _panY = y;
        }
    }
}
