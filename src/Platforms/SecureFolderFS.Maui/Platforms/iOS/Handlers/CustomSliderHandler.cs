using CoreGraphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace SecureFolderFS.Maui.Handlers
{
    internal sealed class CustomSliderHandler : SliderHandler
    {
        private const float NORMAL_HEIGHT = 6f;
        private const float THUMB_COMPENSATION_PADDING = 12f;
        private const float PRESSED_HEIGHT = NORMAL_HEIGHT * 1.8f;

        private UIImage? _trackMin;
        private UIImage? _trackMax;
        private UIImage? _transparentThumb;

        /// <inheritdoc/>
        protected override void ConnectHandler(UISlider slider)
        {
            base.ConnectHandler(slider);

            CreateImages();
            ApplySliderStyles(slider);

            slider.TouchDown += Slider_TouchDown;
            slider.TouchUpInside += Slider_TouchUpInside;
            slider.TouchUpOutside += Slider_TouchUpOutside;
            slider.TouchCancel += Slider_TouchCancel;
        }
        
        /// <inheritdoc/>
        protected override void DisconnectHandler(UISlider slider)
        {
            base.DisconnectHandler(slider);
            
            slider.TouchDown -= Slider_TouchDown;
            slider.TouchUpInside -= Slider_TouchUpInside;
            slider.TouchUpOutside -= Slider_TouchUpOutside;
            slider.TouchCancel -= Slider_TouchCancel;
        }

        private void CreateImages()
        {
            // Create track images with normal height
            _trackMin = CreateResizableTrackImage(Colors.White.ToPlatform(), NORMAL_HEIGHT);
            _trackMax = CreateResizableTrackImage(UIColor.LightGray, NORMAL_HEIGHT);
            _transparentThumb = new UIGraphicsImageRenderer(new CGSize(1, 1)).CreateImage(_ => { });
        }

        private void ApplySliderStyles(UISlider slider)
        {
            slider.SetMinTrackImage(_trackMin, UIControlState.Normal);
            slider.SetMaxTrackImage(_trackMax, UIControlState.Normal);
            slider.SetThumbImage(_transparentThumb, UIControlState.Normal);
            slider.SetThumbImage(_transparentThumb, UIControlState.Highlighted);

            // Start with a normal scale
            slider.Transform = CGAffineTransform.MakeScale(1f, 1f);
        }

        private static void AnimateTrackHeight(UISlider? slider, float height)
        {
            if (slider is null)
                return;

            var minTrack = CreateResizableTrackImage(Colors.White.ToPlatform(), height);
            var maxTrack = CreateResizableTrackImage(UIColor.LightGray, height);

            UIView.Transition(slider, 0.2, UIViewAnimationOptions.TransitionCrossDissolve, () =>
            {
                slider.SetMinTrackImage(minTrack, UIControlState.Normal);
                slider.SetMaxTrackImage(maxTrack, UIControlState.Normal);
            }, null!);
        }

        private static UIImage CreateResizableTrackImage(UIColor color, float height)
        {
            var size = new CGSize(height + THUMB_COMPENSATION_PADDING * 2, height);
            var cornerRadius = height / 2f;
            var renderer = new UIGraphicsImageRenderer(size);

            var image = renderer.CreateImage(_ =>
            {
                var rect = new CGRect(THUMB_COMPENSATION_PADDING, 0, height, height);
                var path = UIBezierPath.FromRoundedRect(rect, cornerRadius);
                color.SetFill();
                path.Fill();
            });

            var capInsets = new UIEdgeInsets(0, size.Width / 2f - 1, 0, size.Width / 2f - 1);
            return image.CreateResizableImage(capInsets, UIImageResizingMode.Stretch);
        }
        
        #region Handlers

        private static void Slider_TouchDown(object? sender, EventArgs e)
            => AnimateTrackHeight(sender as UISlider, PRESSED_HEIGHT);

        private static void Slider_TouchUpInside(object? sender, EventArgs e)
            => AnimateTrackHeight(sender as UISlider, NORMAL_HEIGHT);

        private static void Slider_TouchUpOutside(object? sender, EventArgs e)
            => AnimateTrackHeight(sender as UISlider, NORMAL_HEIGHT);

        private static void Slider_TouchCancel(object? sender, EventArgs e)
            => AnimateTrackHeight(sender as UISlider, NORMAL_HEIGHT);

        #endregion
    }
}
