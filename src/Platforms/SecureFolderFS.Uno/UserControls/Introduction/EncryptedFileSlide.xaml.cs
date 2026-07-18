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
#if HAS_UNO_SKIA || __UNO_SKIA_MACOS__ || __UNO_SKIA_X11__
using Windows.Foundation;
using Uno.WinUI.Graphics2DSK;
#else
using SkiaSharp.Views.Windows;
#endif

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class EncryptedFileSlide : UserControl, IAsyncInitialize, IDisposable
    {
        private const float LENS_ZOOM = 1.15f;
        private const string UI_ASSEMBLY_NAME = $"{nameof(SecureFolderFS)}.UI";

        private const float VELOCITY_SCALE = 0.00018f; // pixels/sec to deform ratio
        private const float MAX_DEFORM = 0.1f;
        private const float LERP_SPEED = 12f;
        private const float SPRING_STIFFNESS = 280f;
        private const float SPRING_DAMPING = 18f;

        // Logical (DIP) units, converted to physical pixels per frame - the lens
        // occupies the same visual size regardless of screen scaling or resolution
        private const float MAGNIFIER_RADIUS = 60f;

        private bool _isInitialized;

#if HAS_UNO_SKIA || __UNO_SKIA_MACOS__ || __UNO_SKIA_X11__
        private readonly LensCanvas _canvas;
#else
        private readonly SKSwapChainPanel _canvas;
#endif

        private SKBitmap? _wallpaperBitmap;
        private SKBitmap? _hexBitmap;
        private SKPoint? _pointerPosition;

        // The lens samples the composed scene, so the base layers are rendered into an
        // offscreen surface whose snapshot can be read back (the on-screen canvas cannot be)
        private SKSurface? _sceneSurface;
        private SKSizeI _sceneSize;
        private GRContext? _sceneContext;

        // Draws the glass disc with per-pixel refraction
        private readonly GlassLensRenderer _lensRenderer = new();

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

#if HAS_UNO_SKIA || __UNO_SKIA_MACOS__ || __UNO_SKIA_X11__
            // SKCanvasElement renders in the app's composition pass (GPU-backed, no
            // per-frame bitmap upload), unlike SKXamlCanvas which lags on desktop
            _canvas = new LensCanvas(this);
#else
            // ANGLE-backed swap chain: Skia draws through a GPU context instead of SKXamlCanvas'
            // per-frame CPU raster + bitmap upload. The panel composes opaquely (no per-pixel
            // transparency over the XAML behind it), which is safe here because the hex and
            // wallpaper layers cover every canvas pixel with opaque content
            _canvas = new SKSwapChainPanel();
            _canvas.PaintSurface += SkiaCanvas_PaintSurface;
#endif
            _canvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            _canvas.VerticalAlignment = VerticalAlignment.Stretch;
            SlotGrid.Children.Add(_canvas);

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
#if !__UNO_SKIA_X11__
                ThemeType.Light => "intro_hex_light.png",
#endif
                _ => "intro_hex_dark.png"
            });
            if (_wallpaperBitmap is not null)
            {
                _wallpaperBitmap = RotateBitmap(_wallpaperBitmap, 180);
                _wallpaperBitmap = FlipBitmap(_wallpaperBitmap, horizontal: true, vertical: true);
            }

            _canvas.Invalidate();

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

#if HAS_UNO_SKIA || __UNO_SKIA_MACOS__ || __UNO_SKIA_X11__
        private sealed class LensCanvas(EncryptedFileSlide owner) : SKCanvasElement
        {
            protected override void RenderOverride(SKCanvas canvas, Size area)
            {
                // Work in physical pixels like SKXamlCanvas did, so that all the tuned
                // radii, stroke widths, and blur sigmas keep their meaning
                var scale = (float)(owner.XamlRoot?.RasterizationScale ?? 1.0);
                canvas.Save();
                canvas.Scale(1f / scale);
                owner.RenderSlide(canvas, (float)area.Width * scale, (float)area.Height * scale, scale);
                canvas.Restore();
            }
        }
#else
        private void SkiaCanvas_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // Display scaling, not canvas-to-control ratio: the canvas only fills the
            // left slot, so dividing by this control's width would shrink the lens
            var scale = (float)(XamlRoot?.RasterizationScale ?? 1.0);
            RenderSlide(canvas, e.Info.Width, e.Info.Height, scale, _canvas.GRContext);
        }
#endif

        private void RenderSlide(SKCanvas canvas, float width, float height, float scale, GRContext? context = null)
        {
            if (width < 1f || height < 1f)
                return;

            var radius = MAGNIFIER_RADIUS * scale;

            var center = _smoothPosition != default ? _smoothPosition : _pointerPosition ?? new SKPoint(width / 2f, height / 2f);
            var sceneSize = new SKSizeI((int)width, (int)height);
            if (_sceneSurface is null || sceneSize != _sceneSize || !ReferenceEquals(_sceneContext, context))
            {
                _sceneSurface?.Dispose();

                // When the destination canvas is GPU-backed, the scene must live on the same
                // context - a raster scene would force a full readback and upload every frame
                _sceneSurface = context is null ? null : SKSurface.Create(context, true, new SKImageInfo(sceneSize.Width, sceneSize.Height));
                _sceneSurface ??= SKSurface.Create(new SKImageInfo(sceneSize.Width, sceneSize.Height));
                _sceneSize = sceneSize;
                _sceneContext = context;
            }

            var scene = _sceneSurface.Canvas;
            scene.Clear(SKColors.Transparent);
            var sceneRect = new SKRect(0, 0, width, height);

            // Layer 1: Hex (encrypted content)
            if (_hexBitmap is not null)
            {
                var hexDest = ComputeUniformToFillRect(_hexBitmap.Width, _hexBitmap.Height, sceneSize.Width, sceneSize.Height);
                scene.Save();
                scene.ClipRect(sceneRect);
                scene.DrawBitmap(_hexBitmap, hexDest);
                scene.Restore();
            }

            // Layer 2: Wallpaper + smooth reveal hole
            if (_wallpaperBitmap is not null)
            {
                var wallpaperDest = ComputeUniformToFillRect(_wallpaperBitmap.Width, _wallpaperBitmap.Height,
                    sceneSize.Width, sceneSize.Height);

                scene.SaveLayer();
                scene.DrawBitmap(_wallpaperBitmap, wallpaperDest);

                using var erasePaint = new SKPaint();
                erasePaint.IsAntialias = true;
                erasePaint.BlendMode = SKBlendMode.DstOut;
                // Fully clear until deep into the lens so the magnified interior has no dark
                // vignette (which would read as sphere shading instead of flat glass)
                erasePaint.Shader = SKShader.CreateRadialGradient(center, radius,
                    [new SKColor(0, 0, 0, 255), new SKColor(0, 0, 0, 245), SKColors.Transparent],
                    [0.6f, 0.88f, 1f],
                    SKShaderTileMode.Clamp);

                scene.DrawCircle(center, radius, erasePaint);
                scene.Restore();
            }

            // Snapshot for the lens effect (only once per frame)
            using var snapshot = _sceneSurface.Snapshot();
            canvas.DrawImage(snapshot, 0f, 0f);

            // Layer 3: Liquid Glass Lens
            var (radiusX, radiusY) = ComputeRingRadii(radius);
            _lensRenderer.Draw(canvas, snapshot, center, radius, radiusX, radiusY, LENS_ZOOM * _lensScale);
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

            _canvas.Invalidate();
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
            _lensScale = 1f;
            _scaleVelocity = 0f;
            _scaleTarget = 1f;
            _positionVelocity = new SKPoint(0f, 0f);
            StopAnimation();
            _canvas.Invalidate();
        }

        private void UpdatePointer(PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(SlotGrid).Position;
            var width = (float)SlotGrid.ActualWidth;
            var height = (float)SlotGrid.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

#if HAS_UNO_SKIA || __UNO_SKIA_MACOS__ || __UNO_SKIA_X11__
            // The lens is rendered in physical pixels; map the logical pointer position accordingly
            var scaleX = (float)(XamlRoot?.RasterizationScale ?? 1.0);
            var scaleY = scaleX;
#else
            var scaleX = _canvas.CanvasSize.Width / width;
            var scaleY = _canvas.CanvasSize.Height / height;
#endif

            var clampedX = Math.Clamp((float)pos.X, MAGNIFIER_RADIUS, width - MAGNIFIER_RADIUS);
            var clampedY = Math.Clamp((float)pos.Y, MAGNIFIER_RADIUS, height - MAGNIFIER_RADIUS);

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

            // Detach the canvas so no further composition pass can render this slide.
            // The Skia resources (paints, bitmaps, scene surface) are intentionally not
            // disposed here: a redraw may already be queued, and the GC reclaims them safely.
            SlotGrid.Children.Remove(_canvas);

            _wallpaperBitmap = null;
            _hexBitmap = null;
            _sceneSurface = null;
            _sceneContext = null;
        }
    }
}
