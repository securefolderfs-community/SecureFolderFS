using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.Uno.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class EncryptedFileSlide : UserControl, IAsyncInitialize, IDisposable
    {
        private const float INNER_SHADOW_OFFSET = 6f;
        private const float DEFORM_STRENGTH = 0.25f;
        private const float LENS_ZOOM = 1.3f;
        private const string UI_ASSEMBLY_NAME = $"{nameof(SecureFolderFS)}.UI";
        
        private const float VELOCITY_SCALE = 0.00018f; // pixels/sec to deform ratio
        private const float MAX_DEFORM = 0.1f;
        private const float LERP_SPEED = 12f;
        private const float SPRING_STIFFNESS = 280f;
        private const float SPRING_DAMPING = 18f;

#if HAS_UNO_SKIA
        private const float MAGNIFIER_RADIUS = 115f;
        private const float MOVEMENT_THRESHOLD = 2.5f;
#else
        private const float MAGNIFIER_RADIUS = 80f;
        private const float MOVEMENT_THRESHOLD = 1f;
#endif

        private bool _isInitialized;

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

        // Fluid pointer dynamics
        private SKPoint _targetPosition;
        private SKPoint _smoothPosition;
        private SKPoint _positionVelocity;

        // Spring scale for press/release bounce
        private float _lensScale = 1f;
        private float _scaleVelocity = 0f;
        private float _scaleTarget = 1f;

        // Animation timer (runs only while settling)
        private readonly DispatcherTimer _animTimer;
        private DateTime _lastTick;
        private bool _isAnimating;

        public EncryptedFileSlide()
        {
            InitializeComponent();

            // Pre-create expensive paint objects
            _blurPaint = new SKPaint
            {
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateBlur(4.8f, 4.8f, SKImageFilter.CreateBlur(1f, 1f)) // light + subtle secondary blur
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

            _invisiblePaint = new SKPaint
            {
                IsAntialias = true,
                ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(255, 255, 255, 0), SKBlendMode.SrcOver)
            };

            _outerGlowPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 28f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen
            };

            _iridescentPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 16f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen
            };

            _corePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 6f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen
            };

            _additiveGlowPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 36f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Plus
            };

            _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60 fps
            _animTimer.Tick += AnimTimer_Tick;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized)
                return Task.CompletedTask;

            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == UI_ASSEMBLY_NAME);
            if (assembly is null)
                return Task.CompletedTask;

            var rnd = Random.Shared.Next(1, 4);
            _wallpaperBitmap = LoadBitmap(assembly, $"Introduction.intro_wallpaper{rnd}.jpg");
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

            _isInitialized = true;
            return Task.CompletedTask;
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

            var center = _smoothPosition != default ? _smoothPosition : _pointerPosition ?? new SKPoint(info.Width / 2f, info.Height / 2f);
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
            DrawLiquidGlassLens(canvas, center, snapshot, _lensScale);
        }

        private void DrawLiquidGlassLens(SKCanvas canvas, SKPoint center, SKImage snapshot, float lensScale = 1f)
        {
            var r = MAGNIFIER_RADIUS;
            var (rx, ry) = ComputeRingRadii(r);
            var innerR = r * 0.76f;

            // Approximate inner ellipse with same aspect ratio as outer
            var innerRx = innerR * (rx / r);
            var innerRy = innerR * (ry / r);

            using var ringPath = new SKPath();
            ringPath.AddOval(new SKRect(center.X - rx, center.Y - ry, center.X + rx, center.Y + ry));
            ringPath.AddOval(new SKRect(center.X - innerRx, center.Y - innerRy, center.X + innerRx, center.Y + innerRy));
            ringPath.FillType = SKPathFillType.EvenOdd;

            // Lens Interior with Edge Deformation
            canvas.SaveLayer();
            canvas.ClipPath(ringPath, SKClipOperation.Intersect, true);

            canvas.Save();

            // Center the transform
            canvas.Translate(center.X, center.Y);

            // Base zoom (slightly reduced so deformation is more visible)
            canvas.Scale(LENS_ZOOM * lensScale, LENS_ZOOM * lensScale);

            // Edge Deformation
            // This creates a directional outward push at the four edges
            // We apply a small additional translation based on normalized position
            // This is approximated by drawing the image multiple times with slight offsets

            // 1. Base zoomed content
            canvas.Translate(-center.X, -center.Y);
            canvas.DrawImage(snapshot, 0, 0, _blurPaint);

            // 2. Deformed passes for edge stretch (directional)
            using var deformPaint = new SKPaint();
            deformPaint.IsAntialias = true;
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

            // Deformed ring rect - rx/ry drive horizontal/vertical radius independently
            var edgeRingRect = new SKRect(center.X - rx + 1, center.Y - ry + 1, center.X + rx - 1, center.Y + ry - 1);

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

            // Fresnel bright edge - follows deformed shape
            using var fresnelPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3.5f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Screen,
                Color = SKColors.White.WithAlpha(180)
            };
            var fresnelRect = new SKRect(center.X - rx + 1.5f, center.Y - ry + 1.5f, center.X + rx - 1.5f, center.Y + ry - 1.5f);
            canvas.DrawOval(fresnelRect, fresnelPaint);
            canvas.DrawOval(fresnelRect, _highlightPaint);

            // Outer shadow - a slightly larger oval
            var shadowRect = new SKRect(center.X - rx - 3f, center.Y - ry - 3f, center.X + rx + 3f, center.Y + ry + 3f);
            canvas.DrawOval(shadowRect, _shadowPaint);

            // Specular highlight - arc mapped onto the deformed ellipse
            using var specularPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.8f,
                IsAntialias = true
            };
            specularPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(center.X + rx * 0.55f, center.Y - ry * 0.65f),
                new SKPoint(center.X + rx * 0.95f, center.Y + ry * 0.45f),
                [SKColors.Transparent, SKColors.White.WithAlpha(235), SKColors.Transparent],
                [0f, 0.5f, 1f],
                SKShaderTileMode.Clamp);

            canvas.DrawArc(
                new SKRect(center.X - rx + 6, center.Y - ry + 6, center.X + rx - 6, center.Y + ry - 6),
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

                // Saturate fully and push brightness to near-max
                s = 1f;
                v = Math.Min(1f, v * 2.2f + 0.75f);

                // Spread hues across the full wheel based on sample position
                // so adjacent samples land on visibly different colors
                h = (h + i / (float)sampleCount * 0.35f + 0.04f) % 1f;

                colors[i] = SKColor.FromHsv(h, s, v).WithAlpha(255);
            }

            colors[sampleCount] = colors[0];
            return colors;
        }
        
        /// <summary>
        /// Computes the ellipse radii for the glass ring, combining spring-scale squeeze
        /// with velocity-driven directional squash-and-stretch.
        /// </summary>
        private (float rx, float ry) ComputeRingRadii(float baseRadius)
        {
            // Spring squeeze (uniform, from press/release)
            // _lensScale < 1 = compressed, > 1 = expanded
            // We invert slightly so a zoom-in squashes the ring outward at edges
            var springSquash = 1f + (1f - _lensScale) * 0.55f;
            var uniformR = baseRadius * springSquash;

            // Velocity squash-and-stretch
            // Map speed to a deformation magnitude, capped so it doesn't go wild
            var vx = _positionVelocity.X;
            var vy = _positionVelocity.Y;
            var speed = MathF.Sqrt(vx * vx + vy * vy);

            var deformX = 0f;
            var deformY = 0f;
            if (speed > 1f)
            {
                var raw = Math.Min(speed * VELOCITY_SCALE, MAX_DEFORM);

                // Direction: normalize velocity, project onto axes
                var nx = vx / speed;
                var ny = vy / speed;

                // Stretch along movement axis, squash perpendicular
                deformX = raw * (nx * nx - ny * ny); // positive = wider when moving horizontally
                deformY = raw * (ny * ny - nx * nx); // positive = taller when moving vertically
            }

            return (uniformR * (1f + deformX), uniformR * (1f + deformY));
        }

        /// <summary>
        /// Mixes a color toward white by <paramref name="amount"/> (0–255) for the bright core pass.
        /// </summary>
        private static SKColor LightenColor(SKColor color, byte amount)
        {
            color.ToHsv(out var h, out var s, out var v);

            // Boost value while keeping saturation fully pegged - stays vivid rather than washing out
            v = Math.Min(1f, v + amount / 255f * 1.4f);
            s = Math.Min(1f, s + 0.15f);

            return SKColor.FromHsv(h, s, v).WithAlpha(color.Alpha);
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

        private void AnimTimer_Tick(object? sender, object e)
        {
            var now = DateTime.UtcNow;
            var dt = (float)(now - _lastTick).TotalSeconds;
            _lastTick = now;

            // Clamp dt to avoid large jumps on frame drops
            dt = Math.Min(dt, 0.05f);

            // Fluid pointer lerp
            var lerpFactor = 1f - MathF.Exp(-LERP_SPEED * dt);

            var prevSmooth = _smoothPosition;
            _smoothPosition = new SKPoint(
                _smoothPosition.X + (_targetPosition.X - _smoothPosition.X) * lerpFactor,
                _smoothPosition.Y + (_targetPosition.Y - _smoothPosition.Y) * lerpFactor
            );

            // Track velocity for directional ring deformation
            if (dt > 0f)
            {
                _positionVelocity = new SKPoint(
                    (_smoothPosition.X - prevSmooth.X) / dt,
                    (_smoothPosition.Y - prevSmooth.Y) / dt
                );
            }

            // Spring scale integration (damped harmonic oscillator)
            var displacement = _lensScale - _scaleTarget;
            var springForce = -SPRING_STIFFNESS * displacement;
            var dampingForce = -SPRING_DAMPING * _scaleVelocity;
            _scaleVelocity += (springForce + dampingForce) * dt;
            _lensScale += _scaleVelocity * dt;

            // Determine if we've settled (stop timer to save CPU)
            var positionSettled = MathF.Abs(_smoothPosition.X - _targetPosition.X) < 0.5f
                                  && MathF.Abs(_smoothPosition.Y - _targetPosition.Y) < 0.5f;
            var scaleSettled = MathF.Abs(_lensScale - _scaleTarget) < 0.001f
                               && MathF.Abs(_scaleVelocity) < 0.001f;

            if (positionSettled && scaleSettled)
            {
                _smoothPosition = _targetPosition;
                _lensScale = _scaleTarget;
                _scaleVelocity = 0f;
                _positionVelocity = new SKPoint(0f, 0f);
                StopAnimation();
            }

            // Invalidate edge color cache if smoothed position moved meaningfully
            var moved = MathF.Abs(_smoothPosition.X - prevSmooth.X) > MOVEMENT_THRESHOLD
                        || MathF.Abs(_smoothPosition.Y - prevSmooth.Y) > MOVEMENT_THRESHOLD;
            if (moved)
                _cachedEdgeColors = null;

            SkiaCanvas.Invalidate();
        }

        private void StartAnimation()
        {
            if (_isAnimating)
                return;
            
            _lastTick = DateTime.UtcNow;
            _isAnimating = true;
            _animTimer.Start();
        }

        private void StopAnimation()
        {
            _isAnimating = false;
            _animTimer.Stop();
        }
        
        private void SlotGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Compress the lens on press
            _scaleTarget = 0.9f;
            _scaleVelocity = 0f;
            StartAnimation();
        }

        private void SlotGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // Overshoot on release, then spring settles to 1.0
            _scaleTarget = 1.06f;
            _scaleVelocity = 0f;

            // After a short delay, target moves to 1.0 - spring does the rest
            Task.Delay(80).ContinueWith(_ =>
            {
                _scaleTarget = 1f;
            });
            StartAnimation();
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
            _lensScale = 1f;
            _scaleVelocity = 0f;
            _scaleTarget = 1f;
            _positionVelocity = new SKPoint(0f, 0f);
            StopAnimation();
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
            _targetPosition = newPosition;

            // Seed smooth position on the first enter so there's no pop from (0,0)
            if (_pointerPosition == null)
                _smoothPosition = newPosition;

            _pointerPosition = newPosition;
            StartAnimation();
        }

        /// <inheritdoc/>
        public new void Dispose()
        {
            _animTimer.Stop();
            _animTimer.Tick -= AnimTimer_Tick;
            
#if HAS_UNO_SKIA
            SkiaCanvas.Dispose();
#endif

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
