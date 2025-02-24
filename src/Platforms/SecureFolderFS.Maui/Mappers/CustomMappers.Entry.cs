using Android.Graphics.Drawables.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SecureFolderFS.Maui.UserControls.Common;

#if ANDROID
using Paint = Android.Graphics.Paint;
using ShapeDrawable = Android.Graphics.Drawables.ShapeDrawable;
#endif

namespace SecureFolderFS.Maui.Mappers
{
    public static partial class CustomMappers
    {
        public static void AddEntryMappers()
        {
            EntryHandler.Mapper.AppendToMapping(nameof(IView.Background), (handler, entry) =>
            {
                if (entry is not ModernEntry)
                    return;
#if ANDROID
                var outerRadii = Enumerable.Range(1, 8).Select(_ => 24f).ToArray();
                var roundRectShape = new RoundRectShape(outerRadii, null, null);
                var shape = new ShapeDrawable(roundRectShape);
                    
                shape.Paint!.Color = (App.Current.Resources["BorderLightColor"] as Color).ToPlatform();
                shape.Paint.StrokeWidth = 5;
                shape.Paint.SetStyle(Paint.Style.Stroke);
                handler.PlatformView.Background = shape;
                handler.PlatformView.SetPadding(40, 32,40, 32);
#elif IOS
                handler.PlatformView.Layer.BorderColor = (App.Current.Resources["BorderLightColor"] as Color).ToPlatform();
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
#endif
            });
        }
    }
}
