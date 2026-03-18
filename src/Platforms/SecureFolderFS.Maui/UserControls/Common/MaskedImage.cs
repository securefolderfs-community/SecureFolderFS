using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
#if ANDROID
using Android.Graphics;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif
using Point = Microsoft.Maui.Graphics.Point;

namespace SecureFolderFS.Maui.UserControls.Common
{
    public sealed class MaskedImage : ContentView
    {
        private SKBitmap? _bitmap;
        private readonly SKCanvasView _canvasView;

        public MaskedImage()
        {
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnPaintSurface;
            Content = _canvasView;
        }

        private static async void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not MaskedImage control)
                return;

            if (newValue is not string fileName)
                return;

#if ANDROID
            var rid = MauiApplication.Current.GetDrawableId(fileName);
            await using var stream = LoadImageFromResources(rid);
#elif IOS || MACCATALYST
            await using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
#endif
            if (stream is not null)
                control._bitmap = SKBitmap.Decode(stream);

            control._canvasView.InvalidateSurface();
        }

#if ANDROID
        private static Stream? LoadImageFromResources(int resourceId)
        {
            var bitmap = BitmapFactory.DecodeResource(MauiApplication.Current.Resources, resourceId);
            if (bitmap is null)
                return null;

            var stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png!, 100, stream);
            stream.Position = 0L;

            return stream;
        }
#endif

        private static void OnStopsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MaskedImage control)
                control._canvasView.InvalidateSurface();
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.Transparent);

            if (_bitmap is null)
                return;

            // Calculate draw rect based on Aspect
            var drawRect = GetDrawRect(_bitmap, new SKRect(0, 0, info.Width, info.Height), Aspect);

            // SaveLayer creates an offscreen buffer so DstIn composites correctly
            using var layerPaint = new SKPaint();
            canvas.SaveLayer(layerPaint);

            // Draw image
            canvas.DrawBitmap(_bitmap, drawRect);

            // Build gradient mask
            var startX = (float)(StartPoint.X * drawRect.Width) + drawRect.Left;
            var startY = (float)(StartPoint.Y * drawRect.Height) + drawRect.Top;
            var endX = (float)(EndPoint.X * drawRect.Width) + drawRect.Left;
            var endY = (float)(EndPoint.Y * drawRect.Height) + drawRect.Top;

            var colors = Stops.OrderBy(s => s.Offset).Select(s =>
            {
                var c = s.Color;
                return new SKColor(
                    (byte)(c.Red * 255),
                    (byte)(c.Green * 255),
                    (byte)(c.Blue * 255),
                    (byte)(c.Alpha * 255));
            }).ToArray();
            var positions = Stops.OrderBy(s => s.Offset).Select(s => s.Offset).ToArray();

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(startX, startY),
                new SKPoint(endX, endY),
                colors,
                positions,
                SKShaderTileMode.Clamp);

            using var maskPaint = new SKPaint();
            maskPaint.Shader = shader;
            maskPaint.BlendMode = SKBlendMode.DstIn;
            canvas.DrawRect(drawRect, maskPaint);

            canvas.Restore();
        }

        private static SKRect GetDrawRect(SKBitmap bmp, SKRect container, Aspect aspect)
        {
            var imgAspect = (float)bmp.Width / bmp.Height;
            var containerAspect = container.Width / container.Height;

            switch (aspect)
            {
                case Aspect.Fill:
                    return container;

                case Aspect.AspectFit:
                {
                    if (imgAspect > containerAspect)
                    {
                        var width = container.Width;
                        var height = width / imgAspect;
                        var y = container.Top + (container.Height - height) / 2;
                        return new SKRect(container.Left, y, container.Left + width, y + height);
                    }
                    else
                    {
                        var height = container.Height;
                        var width = height * imgAspect;
                        var x = container.Left + (container.Width - width) / 2;
                        return new SKRect(x, container.Top, x + width, container.Top + height);
                    }
                }

                case Aspect.AspectFill:
                default:
                {
                    if (imgAspect > containerAspect)
                    {
                        var height = container.Height;
                        var width = height * imgAspect;
                        var x = container.Left + (container.Width - width) / 2;
                        return new SKRect(x, container.Top, x + width, container.Top + height);
                    }
                    else
                    {
                        var width = container.Width;
                        var height = width / imgAspect;
                        var y = container.Top + (container.Height - height) / 2;
                        return new SKRect(container.Left, y, container.Left + width, y + height);
                    }
                }
            }
        }

        public IList<GradientStop> Stops
        {
            get => (IList<GradientStop>)GetValue(StopsProperty);
            set => SetValue(StopsProperty, value);
        }
        public static readonly BindableProperty StopsProperty =
            BindableProperty.Create(nameof(Stops), typeof(IList<GradientStop>), typeof(MaskedImage),
                new List<GradientStop>(), propertyChanged: OnStopsChanged);

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }
        public static readonly BindableProperty StartPointProperty =
            BindableProperty.Create(nameof(StartPoint), typeof(Point), typeof(MaskedImage));

        public Aspect Aspect
        {
            get => (Aspect)GetValue(AspectProperty);
            set => SetValue(AspectProperty, value);
        }
        public static readonly BindableProperty AspectProperty =
            BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(MaskedImage), Aspect.AspectFit);

        public Point EndPoint
        {
            get => (Point)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }
        public static readonly BindableProperty EndPointProperty =
            BindableProperty.Create(nameof(EndPoint), typeof(Point), typeof(MaskedImage));

        public string? SourceFileName
        {
            get => (string?)GetValue(SourceFileNameProperty);
            set => SetValue(SourceFileNameProperty, value);
        }
        public static readonly BindableProperty SourceFileNameProperty =
            BindableProperty.Create(nameof(SourceFileName), typeof(string), typeof(MaskedImage),
                propertyChanged: OnSourceChanged);
    }
}
