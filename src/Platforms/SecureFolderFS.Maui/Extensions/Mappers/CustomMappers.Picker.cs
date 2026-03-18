using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.Maui.Helpers;
#if ANDROID
using Android.Graphics.Drawables.Shapes;
using Paint = Android.Graphics.Paint;
using ShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
#elif IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

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
                handler.PlatformView.Background = shape;
                handler.PlatformView.SetPadding(32, 24, 32, 24);

                void ApplyAndroidColors()
                {
                    SafetyHelpers.NoFailure(() =>
                    {
                        if (!modernPicker.IsTransparent)
                            shape.Paint!.Color = (App.Instance.Resources["ThemeSecondaryColorBrush"] as SolidColorBrush)!.Color.ToPlatform();

                        handler.PlatformView.SetTextColor((App.Instance.Resources[MauiThemeHelper.Instance.ActualTheme switch
                        {
                            ThemeType.Dark => "QuarternaryDarkColor",
                            _ => "QuarternaryLightColor"
                        }] as Color)!.ToPlatform());
                    });
                }

                ApplyAndroidColors();

                void OnThemeChangedAndroid(object? s, EventArgs _) => ApplyAndroidColors();
                void OnUnloadedAndroid(object? s, EventArgs _)
                {
                    MauiThemeHelper.Instance.ActualThemeChanged -= OnThemeChangedAndroid;
                    modernPicker.Unloaded -= OnUnloadedAndroid;
                }

                MauiThemeHelper.Instance.ActualThemeChanged += OnThemeChangedAndroid;
                modernPicker.Unloaded += OnUnloadedAndroid;

#elif IOS || MACCATALYST
                var uiTextField = handler.PlatformView;

                // Remove border
                uiTextField.BorderStyle = UITextBorderStyle.None;
                uiTextField.Layer.CornerRadius = 8f;
                uiTextField.Layer.MasksToBounds = true;

                // Add the chevron icon on the right
                var chevronConfig = UIImageSymbolConfiguration.Create(UIImageSymbolScale.Small);
                var chevronImage = UIImage.GetSystemImage("chevron.up.chevron.down", chevronConfig);
                var chevronImageView = new UIImageView(chevronImage)
                {
                    ContentMode = UIViewContentMode.ScaleAspectFit
                };

                // Create a container for the chevron with padding
                var rightView = new UIView(new CGRect(0, 0, 30, 20));
                chevronImageView.Frame = new CGRect(6, 2, 16, 16);
                rightView.AddSubview(chevronImageView);

                uiTextField.RightView = rightView;
                uiTextField.RightViewMode = UITextFieldViewMode.Always;

                void ApplyIosColors()
                {
                    SafetyHelpers.NoFailure(() =>
                    {
                        uiTextField.BackgroundColor = modernPicker.IsTransparent
                            ? UIColor.Clear
                            : (App.Instance.Resources["ThemeSecondaryColorBrush"] as SolidColorBrush)!.Color.ToPlatform();

                        uiTextField.TextColor = (App.Instance.Resources[MauiThemeHelper.Instance.ActualTheme switch
                        {
                            ThemeType.Dark => "PrimaryContrastingDarkColor",
                            _ => "PrimaryContrastingLightColor"
                        }] as Color)!.ToPlatform();

                        chevronImageView.TintColor = uiTextField.TextColor;
                    });
                }

                ApplyIosColors();

                void OnThemeChangedIos(object? s, EventArgs _) => ApplyIosColors();
                void OnUnloadedIos(object? s, EventArgs _)
                {
                    MauiThemeHelper.Instance.ActualThemeChanged -= OnThemeChangedIos;
                    modernPicker.Unloaded -= OnUnloadedIos;
                }

                MauiThemeHelper.Instance.ActualThemeChanged += OnThemeChangedIos;
                modernPicker.Unloaded += OnUnloadedIos;
#else
                modernPicker.BackgroundColor = modernPicker.IsTransparent
                    ? Colors.Transparent
                    : (App.Instance.Resources["ThemeSecondaryColorBrush"] as SolidColorBrush)!.Color;
#endif
            });
        }
    }
}
