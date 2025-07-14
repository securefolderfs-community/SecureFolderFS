using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.UI.Enums;
#if ANDROID
using Android.Graphics.Drawables.Shapes;
using SecureFolderFS.Maui.Helpers;
using Paint = Android.Graphics.Paint;
using ShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
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
                handler.PlatformView.SetTextColor((App.Instance.Resources[MauiThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Dark => "QuarternaryDarkColor",
                    _ => "QuarternaryLightColor"
                }] as Color)!.ToPlatform());
                handler.PlatformView.Background = shape;
                handler.PlatformView.SetPadding(32, 24, 32, 24);
#endif
            });
        }
    }
}
