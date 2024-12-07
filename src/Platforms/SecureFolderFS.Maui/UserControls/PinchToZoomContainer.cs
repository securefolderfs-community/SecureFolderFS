namespace SecureFolderFS.Maui.UserControls
{
    internal sealed class PinchToZoomContainer : ContentView
    {
        private double _currentScale = 1;
        private double _startScale = 1;
        private double _xOffset;
        private double _yOffset;
        private bool _secondDoubleTap;

        public PinchToZoomContainer()
        {
            var pinchGesture = new PinchGestureRecognizer();
            var panGesture = new PanGestureRecognizer();
            var tapGesture = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };
            
            pinchGesture.PinchUpdated += PinchGesture_PinchUpdated;
            panGesture.PanUpdated += PanGesture_PanUpdated;
            tapGesture.Tapped += TapGesture_DoubleTapped;
            
            GestureRecognizers.Add(pinchGesture);
            GestureRecognizers.Add(panGesture);
            GestureRecognizers.Add(tapGesture);
        }

        private void PinchGesture_PinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                {
                    // Store the current scale factor applied to the wrapped user interface element,
                    // and zero the components for the center point of the translate transform.
                    _startScale = Content.Scale;
                    Content.AnchorX = 0;
                    Content.AnchorY = 0;
                    break;
                }
                
                case GestureStatus.Running:
                {
                    // Calculate the scale factor to be applied.
                    _currentScale += (e.Scale - 1) * _startScale;
                    _currentScale = Math.Max(1, _currentScale);

                    // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                    // so get the X pixel coordinate.
                    var renderedX = Content.X + _xOffset;
                    var deltaX = renderedX / Width;
                    var deltaWidth = Width / (Content.Width * _startScale);
                    var originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                    // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                    // so get the Y pixel coordinate.
                    var renderedY = Content.Y + _yOffset;
                    var deltaY = renderedY / Height;
                    var deltaHeight = Height / (Content.Height * _startScale);
                    var originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                    // Calculate the transformed element pixel coordinates.
                    var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
                    var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

                    // Apply translation based on the change in origin.
                    Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
                    Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

                    // Apply scale factor
                    Content.Scale = _currentScale;
                    break;
                }

                case GestureStatus.Completed:
                {
                    // Store the translation delta's of the wrapped user interface element.
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
                }
            }
        }
        
        public void PanGesture_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (Content.Scale == 1d)
                return;

            var currentPage = Shell.Current.CurrentPage;
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                {
                    var newX = (e.TotalX * Scale) + _xOffset;
                    var newY = (e.TotalY * Scale) + _yOffset;

                    var width = (Content.Width * Content.Scale);
                    var height = (Content.Height * Content.Scale);

                    var canMoveX = width > currentPage.Width;
                    var canMoveY = height > currentPage.Height;

                    if (canMoveX)
                    {
                        var minX = (width - (currentPage.Width / 2)) * -1;
                        var maxX = Math.Min(currentPage.Width / 2, width / 2);

                        if (newX < minX)
                            newX = minX;

                        if (newX > maxX)
                            newX = maxX;
                    }
                    else
                        newX = 0;

                    if (canMoveY)
                    {
                        var minY = (height - (currentPage.Height / 2)) * -1;
                        var maxY = Math.Min(currentPage.Width / 2, height / 2);

                        if (newY < minY)
                            newY = minY;

                        if (newY > maxY)
                            newY = maxY;
                    }
                    else
                        newY = 0;

                    Content.TranslationX = newX;
                    Content.TranslationY = newY;

                    break;
                }

                case GestureStatus.Completed:
                {
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
                }
                
                case GestureStatus.Started:
                case GestureStatus.Canceled:
                    break;
                
                default: throw new ArgumentOutOfRangeException(nameof(e.StatusType));
            }
        }

        public async void TapGesture_DoubleTapped(object? sender, EventArgs e)
        {
            var multiplier = Math.Pow(2d, 1d / 10d);
            _startScale = Content.Scale;
            Content.AnchorX = 0;
            Content.AnchorY = 0;

            for (var i = 0; i < 10; i++)
            {
                if (!_secondDoubleTap) // If it's not the second double tap we enlarge the scale
                {
                    _currentScale *= multiplier;
                }
                else // If it's the second double tap we make the scale smaller again 
                {
                    _currentScale /= multiplier;
                }

                var renderedX = Content.X + _xOffset;
                var deltaX = renderedX / Width;
                var deltaWidth = Width / (Content.Width * _startScale);
                var originX = (0.5 - deltaX) * deltaWidth;

                var renderedY = Content.Y + _yOffset;
                var deltaY = renderedY / Height;
                var deltaHeight = Height / (Content.Height * _startScale);
                var originY = (0.5 - deltaY) * deltaHeight;

                var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
                var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

                Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
                Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

                Content.Scale = _currentScale;
                await Task.Delay(10);
            }
            
            _secondDoubleTap = !_secondDoubleTap;
            _xOffset = Content.TranslationX;
            _yOffset = Content.TranslationY;
        }
    }
}
