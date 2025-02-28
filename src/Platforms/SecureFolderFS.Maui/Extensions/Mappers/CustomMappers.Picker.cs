using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.UserControls.Common;
#if ANDROID
using Android.Graphics.Drawables.Shapes;
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
                var outerRadii = Enumerable.Range(1, 8).Select(_ => 24f).ToArray();
                var roundRectShape = new RoundRectShape(outerRadii, null, null);
                var shape = new ShapeDrawable(roundRectShape);

                shape.Paint!.Color = modernPicker.IsTransparent
                    ? Colors.Transparent.ToPlatform()
                    : (App.Instance.Resources["ThemeSecondaryColorBrush"] as SolidColorBrush)!.Color.ToPlatform();
                shape.Paint.StrokeWidth = 0;
                shape.Paint.SetStyle(Paint.Style.FillAndStroke);
                handler.PlatformView.SetTextColor((App.Instance.Resources["QuarternaryLightColor"] as Color)!.ToPlatform());
                handler.PlatformView.Background = shape;
                handler.PlatformView.SetPadding(32, 24, 32, 24);
#endif
            });
        }
    }
}
