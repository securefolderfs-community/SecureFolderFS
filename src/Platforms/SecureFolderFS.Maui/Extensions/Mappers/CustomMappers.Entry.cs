using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.UI.Enums;
#if ANDROID
using Android.Graphics.Drawables.Shapes;
using Paint = Android.Graphics.Paint;
using ShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
#endif

namespace SecureFolderFS.Maui.Extensions.Mappers
{
    public static partial class CustomMappers
    {
        public static void AddEntryMappers()
        {
            EntryHandler.Mapper.AppendToMapping($"{nameof(CustomMappers)}.{nameof(Entry)}", (handler, view) =>
            {
                if (view is not ModernEntry)
                    return;
#if ANDROID
                const float R = 24f;
                var outerRadii = new[] { R, R, R, R, R, R, R, R };
                var roundRectShape = new RoundRectShape(outerRadii, null, null);
                var shape = new ShapeDrawable(roundRectShape);

                shape.Paint!.Color = (App.Instance.Resources[MauiThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Dark => "BorderDarkColor",
                    _ => "BorderLightColor"
                }] as Color)!.ToPlatform();
                shape.Paint.StrokeWidth = 4;
                shape.Paint.SetStyle(Paint.Style.Stroke);
                handler.PlatformView.Background = shape;
                handler.PlatformView.SetPadding(40, 32,40, 32);
#elif IOS
                handler.PlatformView.Layer.BorderColor = (App.Current.Resources[MauiThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Dark => "BorderDarkColor",
                    _ => "BorderLightColor"
                }] as Color).ToPlatform().CGColor;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
#endif
            });
        }
    }
}
