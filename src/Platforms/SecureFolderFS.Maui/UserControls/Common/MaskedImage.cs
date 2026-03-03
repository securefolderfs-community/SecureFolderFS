using Microsoft.Maui.Graphics.Platform;
using BlendMode = Microsoft.Maui.Graphics.BlendMode;
using IImage = Microsoft.Maui.Graphics.IImage;
using Point = Microsoft.Maui.Graphics.Point;
using RectF = Microsoft.Maui.Graphics.RectF;
#if ANDROID
using Android.Graphics;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif

namespace SecureFolderFS.Maui.UserControls.Common
{
    public sealed class MaskedImage : GraphicsView
    {
        internal IImage? ImageInternal { get; private set; }

        public MaskedImage()
        {
            Drawable = new MaskedImageDrawable(this);
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
            control.ImageInternal = PlatformImage.FromStream(stream);
            control.Invalidate(); // Trigger redraw
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
            (bindable as MaskedImage)?.Invalidate();
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

    internal sealed class MaskedImageDrawable : IDrawable
    {
        private readonly MaskedImage _control;

        public MaskedImageDrawable(MaskedImage control)
        {
            _control = control;
        }

        public void Draw(ICanvas canvas, RectF r)
        {
            if (_control.ImageInternal is null)
                return;

            // Draw image
            r = GetDrawRect(_control.ImageInternal, r, _control.Aspect);
            canvas.DrawImage(_control.ImageInternal, r.X, r.Y, r.Width, r.Height);

            // Build gradient
            var gradient = new LinearGradientPaint
            {
                StartPoint = _control.StartPoint,
                EndPoint = _control.EndPoint,
                GradientStops = _control.Stops.Select(x => new PaintGradientStop(x.Offset, x.Color)).ToArray()
            };

            // Apply mask with DestinationIn blend mode
            canvas.BlendMode = BlendMode.DestinationIn;
            canvas.SetFillPaint(gradient, r);
            canvas.FillRectangle(r);

            // Reset
            canvas.BlendMode = BlendMode.Normal;
        }

        private static RectF GetDrawRect(IImage img, RectF container, Aspect aspect)
        {
            var imgAspect = img.Width / img.Height;
            var containerAspect = container.Width / container.Height;

            switch (aspect)
            {
                case Aspect.Fill:
                {
                    // Stretch to fill container
                    return container;
                }

                case Aspect.AspectFit:
                {

                    if (imgAspect > containerAspect)
                    {
                        // Image is wider
                        var width = container.Width;
                        var height = width / imgAspect;
                        var y = container.Y + (container.Height - height) / 2;

                        return new RectF(container.X, y, width, height);
                    }
                    else
                    {
                        // Image is taller
                        var height = container.Height;
                        var width = height * imgAspect;
                        var x = container.X + (container.Width - width) / 2;

                        return new RectF(x, container.Y, width, height);
                    }
                }

                case Aspect.AspectFill:
                default:
                {
                    if (imgAspect > containerAspect)
                    {
                        // Image is wider
                        var height = container.Height;
                        var width = height * imgAspect;
                        var x = container.X + (container.Width - width) / 2;

                        return new RectF(x, container.Y, width, height);
                    }
                    else
                    {
                        // Image is taller
                        var width = container.Width;
                        var height = width / imgAspect;
                        var y = container.Y + (container.Height - height) / 2;

                        return new RectF(container.X, y, width, height);
                    }
                }
            }
        }
    }
}
