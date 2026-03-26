using System;
using System.Linq;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class EncryptedFileSlide : UserControl
    {
        private const float MAGNIFIER_RADIUS = 80f;
        private const string UI_ASSEMBLY_NAME = $"{nameof(SecureFolderFS)}.UI";

        private SKBitmap? _wallpaperBitmap;
        private SKBitmap? _hexBitmap;
        private SKPoint? _pointerPosition;

        // Reusable resources for better performance
        private readonly SKPaint _blurPaint;
        private readonly SKPaint _ringPaint;
        private readonly SKPaint _highlightPaint;
        private readonly SKPaint _shadowPaint;

        public EncryptedFileSlide()
        {
            InitializeComponent();

            // Pre-create expensive paint objects
            _blurPaint = new SKPaint
            {
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateBlur(5.5f, 5.5f, SKImageFilter.CreateBlur(1f, 1f)) // light + subtle secondary blur
            };

            _ringPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White.WithAlpha(180),
                StrokeWidth = 2f,
                IsAntialias = true
            };

            _highlightPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White.WithAlpha(140),
                StrokeWidth = 1.8f,
                IsAntialias = true
            };

            _shadowPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(0, 0, 0, 45),
                StrokeWidth = 5f,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3.5f)
            };
        }

        private void EncryptedFileSlide_Loaded(object sender, RoutedEventArgs e)
        {
            var assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .SingleOrDefault(x => x.GetName().Name == UI_ASSEMBLY_NAME);

            if (assembly is null)
                return;

            _wallpaperBitmap = LoadBitmap(assembly, "Introduction.intro_wallpaper.jpg");
            _hexBitmap = LoadBitmap(assembly, "Introduction.intro_hex_dark.png");
            if (_wallpaperBitmap is not null)
            {
                _wallpaperBitmap = RotateBitmap(_wallpaperBitmap, 180);
                _wallpaperBitmap = FlipBitmap(_wallpaperBitmap, horizontal: true, vertical: true);
            }

            SkiaCanvas.Invalidate();
        }

        private static SKBitmap? LoadBitmap(Assembly assembly, string resourceSuffix)
        {
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains($"Assets.AppAssets.{resourceSuffix}"));
            if (resourceName is null)
                return null;

            using var stream = assembly.GetManifestResourceStream(resourceName);
            return stream is null ? null : SKBitmap.Decode(stream);
        }

        private static SKBitmap RotateBitmap(SKBitmap src, int degrees)
        {
            var isSwapped = degrees is 90 or 270;
            var result = new SKBitmap(isSwapped ? src.Height : src.Width, isSwapped ? src.Width : src.Height);

            using var canvas = new SKCanvas(result);
            canvas.Translate(result.Width / 2f, result.Height / 2f);
            canvas.RotateDegrees(degrees);
            canvas.Translate(-src.Width / 2f, -src.Height / 2f);
            canvas.DrawBitmap(src, 0, 0);

            return result;
        }

        private static SKBitmap FlipBitmap(SKBitmap src, bool horizontal, bool vertical)
        {
            var result = new SKBitmap(src.Width, src.Height);
            using var canvas = new SKCanvas(result);

            var matrix = SKMatrix.CreateIdentity();
            if (horizontal)
                matrix = matrix.PostConcat(SKMatrix.CreateScale(-1, 1, src.Width / 2f, 0));
            
            if (vertical)
                matrix = matrix.PostConcat(SKMatrix.CreateScale(1, -1, 0, src.Height / 2f));

            canvas.SetMatrix(matrix);
            canvas.DrawBitmap(src, 0, 0);
            return result;
        }

        private void SkiaCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.Transparent);

            var center = _pointerPosition ?? new SKPoint(info.Width / 2f, info.Height / 2f);
            var canvasRect = new SKRect(0, 0, info.Width, info.Height);

            // Layer 1: Hex (encrypted content)
            if (_hexBitmap is not null)
            {
                var hexDest = ComputeUniformToFillRect(_hexBitmap.Width, _hexBitmap.Height, info.Width, info.Height);
                canvas.Save();
                canvas.ClipRect(canvasRect);
                canvas.DrawBitmap(_hexBitmap, hexDest);
                canvas.Restore();
            }

            // Layer 2: Wallpaper + smooth reveal hole
            if (_wallpaperBitmap is not null)
            {
                var wallpaperDest = ComputeUniformToFillRect(_wallpaperBitmap.Width, _wallpaperBitmap.Height,
                    info.Width, info.Height);

                canvas.SaveLayer();
                canvas.DrawBitmap(_wallpaperBitmap, wallpaperDest);

                using var erasePaint = new SKPaint();
                erasePaint.IsAntialias = true;
                erasePaint.BlendMode = SKBlendMode.DstOut;
                erasePaint.Shader = SKShader.CreateRadialGradient(center, MAGNIFIER_RADIUS,
                    [new SKColor(0, 0, 0, 255), SKColors.Transparent],
                    [0f, 1f], SKShaderTileMode.Clamp);

                canvas.DrawCircle(center, MAGNIFIER_RADIUS, erasePaint);
                canvas.Restore();
            }

            // Snapshot for the lens effect (only once per frame)
            using var snapshot = e.Surface.Snapshot();

            // Layer 3: Liquid Glass Lens (optimized)
            DrawLiquidGlassLens(canvas, center, snapshot);
        }

        private void DrawLiquidGlassLens(SKCanvas canvas, SKPoint center, SKImage snapshot)
        {
            var r = MAGNIFIER_RADIUS;
            var innerR = r * 0.76f;

            using var ringPath = new SKPath();
            ringPath.AddCircle(center.X, center.Y, r);
            ringPath.AddCircle(center.X, center.Y, innerR);
            ringPath.FillType = SKPathFillType.EvenOdd;

            // Blur only inside the ring + smooth fade
            canvas.SaveLayer();
            canvas.ClipPath(ringPath, SKClipOperation.Intersect, true);
            canvas.DrawImage(snapshot, 0, 0, _blurPaint);

            // Smooth radial fade to avoid hard edges
            using var fadePaint = new SKPaint();
            fadePaint.BlendMode = SKBlendMode.DstIn;
            fadePaint.Shader = SKShader.CreateRadialGradient(center, r,
                [SKColors.Transparent, SKColors.Transparent, new SKColor(255, 255, 255, 220)],
                [0f, innerR / r, 1f],
                SKShaderTileMode.Clamp);
            
            canvas.DrawCircle(center, r, fadePaint);
            canvas.Restore();

            // Glass rim
            canvas.DrawCircle(center, r, _ringPaint);

            // Inner highlight (subtle bevel)
            canvas.DrawCircle(center, r - 2f, _highlightPaint);

            // Outer soft shadow for depth
            canvas.DrawCircle(center, r + 3f, _shadowPaint);
        }

        private static SKRect ComputeUniformToFillRect(int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            var scale = Math.Max((float)dstWidth / srcWidth, (float)dstHeight / srcHeight);
            var scaledW = srcWidth * scale;
            var scaledH = srcHeight * scale;
            var offsetX = (dstWidth - scaledW) / 2f;
            var offsetY = (dstHeight - scaledH) / 2f;

            return new SKRect(offsetX, offsetY, offsetX + scaledW, offsetY + scaledH);
        }

        private void SlotGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            UpdatePointer(e);   
        }

        private void SlotGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            UpdatePointer(e);
        }

        private void SlotGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _pointerPosition = null;
            SkiaCanvas.Invalidate();
        }

        private void UpdatePointer(PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(SlotGrid).Position;
            var width = (float)SlotGrid.ActualWidth;
            var height = (float)SlotGrid.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            var scaleX = SkiaCanvas.CanvasSize.Width / width;
            var scaleY = SkiaCanvas.CanvasSize.Height / height;

            var clampedX = Math.Clamp((float)pos.X, MAGNIFIER_RADIUS / scaleX, width - MAGNIFIER_RADIUS / scaleX);
            var clampedY = Math.Clamp((float)pos.Y, MAGNIFIER_RADIUS / scaleY, height - MAGNIFIER_RADIUS / scaleY);

            _pointerPosition = new SKPoint(clampedX * scaleX, clampedY * scaleY);
            SkiaCanvas.Invalidate();
        }

        private void EncryptedFileSlide_Unloaded(object sender, RoutedEventArgs e)
        {
            _blurPaint.Dispose();
            _ringPaint.Dispose();
            _highlightPaint.Dispose();
            _shadowPaint.Dispose();
        }
    }
}
