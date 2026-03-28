using System;
using System.Linq;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.Uno.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class EncryptedFileSlide : UserControl
    {
        private const float MOVEMENT_THRESHOLD = 2.5f;
        private const float INNER_SHADOW_OFFSET = 6f;
        private const float DEFORM_STRENGTH = 0.25f;
        private const float MAGNIFIER_RADIUS = 115f;
        private const float LENS_ZOOM = 1.3f;
        private const string UI_ASSEMBLY_NAME = $"{nameof(SecureFolderFS)}.UI";

        private SKPoint? _lastInvalidatedPosition;

        private SKBitmap? _wallpaperBitmap;
        private SKBitmap? _hexBitmap;
        private SKPoint? _pointerPosition;

        // Reusable resources for better performance
        private readonly SKPaint _highlightPaint;
        private readonly SKPaint _shadowPaint;
        private readonly SKPaint _blurPaint;

        private SKColor[]? _cachedEdgeColors;
        private SKColor[]? _cachedCoreColors;
        private float[]? _cachedSweepStops;

        private readonly SKPaint _invisiblePaint; // for the warping trick
        private readonly SKPaint _outerGlowPaint;
        private readonly SKPaint _iridescentPaint;
        private readonly SKPaint _corePaint;
        private readonly SKPaint _additiveGlowPaint;

        public EncryptedFileSlide()
        {
            InitializeComponent();

            // Pre-create expensive paint objects
            _blurPaint = new SKPaint
            {
                IsAntialias = true,
                ImageFilter =
                    SKImageFilter.CreateBlur(4.8f, 4.8f,
                        SKImageFilter.CreateBlur(1f, 1f)) // light + subtle secondary blur
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
                Color = new SKColor(0, 0, 0, 30),
                StrokeWidth = 3f,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6.5f)
            };

            _invisiblePaint = new SKPaint { IsAntialias = true };
            _invisiblePaint.ColorFilter = SKColorFilter.CreateBlendMode(
                new SKColor(255, 255, 255, 0), SKBlendMode.SrcOver);

            _outerGlowPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 24f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen
            };

            _iridescentPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 12f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen
            };

            _corePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3.5f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen
            };

            _additiveGlowPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 28f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Plus
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
            _hexBitmap = LoadBitmap(assembly, "Introduction." + UnoThemeHelper.Instance.ActualTheme switch
            {
                ThemeType.Light => "intro_hex_light.png",
                _ => "intro_hex_dark.png"
            });
            if (_wallpaperBitmap is not null)
            {
                _wallpaperBitmap = RotateBitmap(_wallpaperBitmap, 180);
                _wallpaperBitmap = FlipBitmap(_wallpaperBitmap, horizontal: true, vertical: true);
            }

            _cachedEdgeColors = null;
            _cachedSweepStops = null;
            _cachedCoreColors = null;
            SkiaCanvas.Invalidate();
        }

        private static SKBitmap? LoadBitmap(Assembly assembly, string resourceSuffix)
        {
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.Contains($"Assets.AppAssets.{resourceSuffix}"));
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
                    [new SKColor(0, 0, 0, 250), new SKColor(0, 0, 0, 200), SKColors.Transparent],
                    [0.2f, 0.4f, 1f],
                    SKShaderTileMode.Clamp);

                canvas.DrawCircle(center, MAGNIFIER_RADIUS, erasePaint);
                canvas.Restore();
            }

            // Snapshot for the lens effect (only once per frame)
            using var snapshot = e.Surface.Snapshot();

            // Layer 3: Liquid Glass Lens
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

            // Lens Interior with Edge Deformation
            canvas.SaveLayer();
            canvas.ClipPath(ringPath, SKClipOperation.Intersect, true);

            canvas.Save();

            // Center the transform
            canvas.Translate(center.X, center.Y);

            // Base zoom (slightly reduced so deformation is more visible)
            canvas.Scale(LENS_ZOOM, LENS_ZOOM);

            // Edge Deformation
            // This creates a directional outward push at the four edges
            // We apply a small additional translation based on normalized position
            // This is approximated by drawing the image multiple times with slight offsets

            // 1. Base zoomed content
            canvas.Translate(-center.X, -center.Y);
            canvas.DrawImage(snapshot, 0, 0, _blurPaint);

            // 2. Deformed passes for edge stretch (directional)
            using var deformPaint = new SKPaint { IsAntialias = true };
            deformPaint.ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(255, 255, 255, 40), SKBlendMode.SrcOver);

            // Top edge - push upward
            canvas.DrawImage(snapshot, 0, DEFORM_STRENGTH * 12, deformPaint);

            // Bottom edge - push downward
            canvas.DrawImage(snapshot, 0, -DEFORM_STRENGTH * 12, deformPaint);

            // Left edge - push left
            canvas.DrawImage(snapshot, DEFORM_STRENGTH * 12, 0, deformPaint);

            // Right edge - push right
            canvas.DrawImage(snapshot, -DEFORM_STRENGTH * 12, 0, deformPaint);

            // Invisible pass to help with warping consistency
            canvas.DrawImage(snapshot, 0, 0, _invisiblePaint);

            canvas.Restore(); // end all transforms

            // Internal Glass Effects
            
            // Caustic light scattering
            var causticPhase = (float)(DateTime.UtcNow.TimeOfDay.TotalSeconds * 0.8) % (MathF.PI * 2);
            using var causticPaint = new SKPaint
            {
                BlendMode = SKBlendMode.Screen,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(center.X + MathF.Sin(causticPhase) * 12,
                        center.Y + MathF.Cos(causticPhase) * 12),
                    r * 0.45f,
                    [SKColors.White.WithAlpha(0), SKColors.White.WithAlpha(90), SKColors.White.WithAlpha(0)],
                    [0.3f, 0.7f, 1f], SKShaderTileMode.Clamp)
            };
            canvas.DrawCircle(center, r * 0.65f, causticPaint);

            // Dynamic inner shadow for thickness
            using var innerShadowPaint = new SKPaint
            {
                BlendMode = SKBlendMode.DstOut,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(center.X - INNER_SHADOW_OFFSET, center.Y - INNER_SHADOW_OFFSET),
                    r * 0.82f,
                    [new SKColor(0, 0, 0, 0), new SKColor(0, 0, 0, 80)],
                    [0.7f, 1f], SKShaderTileMode.Clamp)
            };
            canvas.DrawCircle(center, r * 0.78f, innerShadowPaint);

            // Smooth radial fade
            using var fadePaint = new SKPaint
            {
                BlendMode = SKBlendMode.DstIn,
                Shader = SKShader.CreateRadialGradient(center, r,
                    [SKColors.Transparent, SKColors.Transparent, new SKColor(255, 255, 255, 235)],
                    [0f, innerR / r, 1f], SKShaderTileMode.Clamp)
            };
            canvas.DrawCircle(center, r, fadePaint);

            canvas.Restore(); // end lens interior SaveLayer

            // Iridescent Rim + Edge Highlights
            var edgeColors = GetRimColors(snapshot, center, r);
            if (_cachedSweepStops == null || _cachedSweepStops.Length != edgeColors.Length)
            {
                _cachedSweepStops = Enumerable.Range(0, edgeColors.Length)
                    .Select(i => i / (float)(edgeColors.Length - 1)).ToArray();
            }

            if (_cachedCoreColors == null || _cachedCoreColors.Length != edgeColors.Length)
                _cachedCoreColors = edgeColors.Select(c => LightenColor(c, 70)).ToArray();

            var edgeRingRect = new SKRect(center.X - r + 1, center.Y - r + 1,
                center.X + r - 1, center.Y + r - 1);

            // Wide outer glow
            _outerGlowPaint.Shader = SKShader.CreateSweepGradient(center, edgeColors, _cachedSweepStops);
            canvas.DrawOval(edgeRingRect, _outerGlowPaint);

            // Vibrant mid ring
            _iridescentPaint.Shader = SKShader.CreateSweepGradient(center, edgeColors, _cachedSweepStops);
            canvas.DrawOval(edgeRingRect, _iridescentPaint);

            // Bright core
            _corePaint.Shader = SKShader.CreateSweepGradient(center, _cachedCoreColors, _cachedSweepStops);
            canvas.DrawOval(edgeRingRect, _corePaint);

            // Aggressive additive glow
            _additiveGlowPaint.Shader = SKShader.CreateSweepGradient(center, edgeColors, _cachedSweepStops);
            canvas.DrawOval(edgeRingRect, _additiveGlowPaint);

            // Fresnel bright edge
            using var fresnelPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3.5f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen,
                Color = SKColors.White.WithAlpha(180)
            };
            canvas.DrawCircle(center, r - 1.5f, fresnelPaint);
            canvas.DrawCircle(center, r - 2f, _highlightPaint);
            canvas.DrawCircle(center, r + 3f, _shadowPaint);

            // Specular highlight
            using var specularPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.8f,
                IsAntialias = true
            };
            specularPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(center.X + r * 0.55f, center.Y - r * 0.65f),
                new SKPoint(center.X + r * 0.95f, center.Y + r * 0.45f),
                [SKColors.Transparent, SKColors.White.WithAlpha(235), SKColors.Transparent],
                [0f, 0.5f, 1f],
                SKShaderTileMode.Clamp);

            canvas.DrawArc(new SKRect(center.X - r + 6, center.Y - r + 6, center.X + r - 6, center.Y + r - 6),
                305, 110, false, specularPaint);
        }

        private SKColor[] GetRimColors(SKImage snapshot, SKPoint center, float radius)
        {
            // Recompute only when necessary (e.g., the pointer moved a lot)
            if (_cachedEdgeColors != null)
                return _cachedEdgeColors;

            _cachedEdgeColors = SampleRimColors(snapshot, center, radius, sampleCount: 20);
            return _cachedEdgeColors;
        }

        /// <summary>
        /// Samples pixel colors from the lens rim and boosts them aggressively for a vivid iridescent effect.
        /// </summary>
        private static SKColor[] SampleRimColors(SKImage image, SKPoint center, float radius, int sampleCount)
        {
            var colors = new SKColor[sampleCount + 1];
            using var bitmap = SKBitmap.FromImage(image);

            for (var i = 0; i < sampleCount; i++)
            {
                var angle = 2f * MathF.PI * i / sampleCount;
                var x = (int)(center.X + radius * MathF.Cos(angle));
                var y = (int)(center.Y + radius * MathF.Sin(angle));

                x = Math.Clamp(x, 0, bitmap.Width - 1);
                y = Math.Clamp(y, 0, bitmap.Height - 1);

                var pixel = bitmap.GetPixel(x, y);
                pixel.ToHsv(out var h, out var s, out var v);

                // Aggressive boost for maximum visibility
                s = Math.Min(1f, s * 5.0f + 0.65f); // very heavy saturation
                v = Math.Min(1f, v * 3.6f + 0.55f); // strong brightness push

                // Gentle hue rotation for a more dynamic color feel
                h = (h + 0.025f) % 1f;

                colors[i] = SKColor.FromHsv(h, s, v).WithAlpha(250);
            }

            colors[sampleCount] = colors[0];
            return colors;
        }

        /// <summary>
        /// Mixes a color toward white by <paramref name="amount"/> (0–255) for the bright core pass.
        /// </summary>
        private static SKColor LightenColor(SKColor color, byte amount)
        {
            return new SKColor(
                (byte)Math.Min(255, color.Red + amount),
                (byte)Math.Min(255, color.Green + amount),
                (byte)Math.Min(255, color.Blue + amount),
                color.Alpha);
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
            _lastInvalidatedPosition = null;
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

            var newPosition = new SKPoint(clampedX * scaleX, clampedY * scaleY);

            // Only invalidate if the pointer moved more than the threshold
            if (_lastInvalidatedPosition == null ||
                MathF.Abs(newPosition.X - _lastInvalidatedPosition.Value.X) > MOVEMENT_THRESHOLD ||
                MathF.Abs(newPosition.Y - _lastInvalidatedPosition.Value.Y) > MOVEMENT_THRESHOLD)
            {
                _pointerPosition = newPosition;
                _lastInvalidatedPosition = newPosition;
                SkiaCanvas.Invalidate();
            }
        }

        private void EncryptedFileSlide_Unloaded(object sender, RoutedEventArgs e)
        {
            _blurPaint.Dispose();
            _highlightPaint.Dispose();
            _shadowPaint.Dispose();

            _invisiblePaint.Dispose();
            _outerGlowPaint.Dispose();
            _iridescentPaint.Dispose();
            _corePaint.Dispose();
            _additiveGlowPaint.Dispose();

            _cachedEdgeColors = null;
            _cachedSweepStops = null;
            _cachedCoreColors = null;
        }
    }
}
