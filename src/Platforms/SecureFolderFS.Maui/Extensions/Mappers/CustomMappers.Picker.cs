#if ANDROID
using Android.Graphics.Drawables.Shapes;
using SecureFolderFS.Maui.Helpers;
using Paint = Android.Graphics.Paint;
using ShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
#elif IOS
using UIKit;
using CoreGraphics;
#endif
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui.Extensions.Mappers
{
    public static partial class CustomMappers
    {
        public static void AddPickerMappers()
        {
            PickerHandler.Mapper.AppendToMapping($"{nameof(CustomMappers)}.{nameof(Picker)}", (handler, view) =>
            {
                if (view is not ModernPicker modernPicker)
                    return;

#if ANDROID
                const float R = 24f;
                var outerRadii = new[] { R, R, R, R, R, R, R, R };
                var roundRectShape = new RoundRectShape(outerRadii, null, null);
                var shape = new ShapeDrawable(roundRectShape);

                shape.Paint!.Color = modernPicker.IsTransparent
                    ? Colors.Transparent.ToPlatform()
                    : (App.Instance.Resources["ThemeSecondaryColorBrush"] as SolidColorBrush)!.Color.ToPlatform();
                shape.Paint.StrokeWidth = 0;
                shape.Paint.SetStyle(Paint.Style.FillAndStroke);
                handler.PlatformView.SetTextColor((App.Instance.Resources[MauiThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Dark => "QuarternaryDarkColor",
                    _ => "QuarternaryLightColor"
                }] as Color)!.ToPlatform());
                handler.PlatformView.Background = shape;
                handler.PlatformView.SetPadding(32, 24, 32, 24);
#elif IOS || MACCATALYST
                var uiTextField = handler.PlatformView;
                
                // Remove border
                uiTextField.BorderStyle = UITextBorderStyle.None;
                
                // Set background color
                uiTextField.BackgroundColor = modernPicker.IsTransparent
                    ? UIColor.Clear
                    : (App.Instance.Resources["ThemeSecondaryColorBrush"] as SolidColorBrush)!.Color.ToPlatform();
                
                // Set text color
                uiTextField.TextColor = (App.Instance.Resources[MauiThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Dark => "PrimaryContrastingDarkColor",
                    _ => "PrimaryContrastingLightColor"
                }] as Color)!.ToPlatform();
                
                // Add the chevron icon on the right
                var chevronConfig = UIImageSymbolConfiguration.Create(UIImageSymbolScale.Small);
                var chevronImage = UIImage.GetSystemImage("chevron.up.chevron.down", chevronConfig);
                var chevronImageView = new UIImageView(chevronImage)
                {
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    TintColor = uiTextField.TextColor
                };
                
                // Create a container for the chevron with padding
                var rightView = new UIView(new CGRect(0, 0, 30, 20));
                chevronImageView.Frame = new CGRect(6, 2, 16, 16);
                rightView.AddSubview(chevronImageView);
                
                uiTextField.RightView = rightView;
                uiTextField.RightViewMode = UITextFieldViewMode.Always;
#else
                modernPicker.BackgroundColor = modernPicker.IsTransparent
                    ? Colors.Transparent
                    : (App.Instance.Resources["ThemeSecondaryColorBrush"] as SolidColorBrush)!.Color;
#endif
            });
        }
    }
}
