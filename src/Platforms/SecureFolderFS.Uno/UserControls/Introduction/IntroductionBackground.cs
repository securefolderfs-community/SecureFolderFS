using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using SkiaSharp;
#if HAS_UNO_SKIA
using Uno.WinUI.Graphics2DSK;
#else
using SkiaSharp.Views.Windows;
#endif

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    /// <summary>
    /// An animated, grainy mesh-gradient background. See <see cref="IntroductionBackgroundRenderer"/>
    /// for the drawing logic; this control hosts it on the fastest canvas available per platform.
    /// </summary>
    public sealed partial class IntroductionBackground : UserControl, IDisposable
    {
        private const double REVEAL_DURATION_MS = 1200d;
        private const double SHADOW_FADE_MS = 350d;

#if HAS_UNO_SKIA
        // SKCanvasElement draws directly in the app's composition pass (GPU-backed,
        // no per-frame bitmap upload), so a full frame rate is affordable
        private const int FRAME_INTERVAL_MS = 16;
        private readonly CompositionCanvas _canvas;
#else
        // The reveal ripple needs true per-pixel transparency over the XAML behind this
        // control, which the ANGLE swap chain cannot compose (it presents opaquely). The
        // reveal therefore runs on a CPU-rasterized SKXamlCanvas at a reduced cadence with
        // cheaper sampling; once the gradient covers every pixel, rendering hands off to a
        // GPU-backed SKSwapChainPanel at full frame rate and quality
        private const int FRAME_INTERVAL_MS = 33;
        private const int GPU_FRAME_INTERVAL_MS = 16;
        private readonly Grid _canvasHost;
        private readonly SKXamlCanvas _revealCanvas;
        private SKSwapChainPanel? _gpuCanvas;
        private bool _revealCanvasRemoved;
#endif

        private readonly IntroductionBackgroundRenderer _renderer = new();
        private readonly DispatcherTimer _frameTimer;
        private readonly Stopwatch _clock = Stopwatch.StartNew();

        private bool _hasRendered;
        private float _revealProgress;
        private float _shadowOpacity;
        private DateTime? _revealStart;
        private DateTime? _shadowStart;
        private TaskCompletionSource? _revealTcs;
        private Rect? _contentBounds;

        public IntroductionBackground()
        {
            IsHitTestVisible = false;

#if HAS_UNO_SKIA
            _canvas = new CompositionCanvas(this);
            _canvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            _canvas.VerticalAlignment = VerticalAlignment.Stretch;
            Content = _canvas;
#else
            _renderer.HighQualitySampling = false;
            _revealCanvas = new SKXamlCanvas();
            _revealCanvas.PaintSurface += Canvas_PaintSurface;
            _revealCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            _revealCanvas.VerticalAlignment = VerticalAlignment.Stretch;

            _canvasHost = new Grid();
            _canvasHost.Children.Add(_revealCanvas);
            Content = _canvasHost;
#endif

            _frameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FRAME_INTERVAL_MS) };
            _frameTimer.Tick += FrameTimer_Tick;

            Loaded += IntroductionBackground_Loaded;
            Unloaded += IntroductionBackground_Unloaded;
            ActualThemeChanged += IntroductionBackground_ActualThemeChanged;
        }

        /// <summary>
        /// Reveals the background with a soft-edged ripple expanding from the center.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the background is fully revealed.</returns>
        public Task RevealAsync()
        {
            if (_revealTcs is not null)
                return _revealTcs.Task;

            _revealTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _revealStart = DateTime.UtcNow;
            _frameTimer.Start();

            return _revealTcs.Task;
        }

        /// <summary>
        /// Sets the bounds (relative to this control) of the elevated content that the
        /// background draws a drop shadow beneath.
        /// </summary>
        public void SetContentBounds(Rect bounds)
        {
            _contentBounds = bounds;
        }

        private void FrameTimer_Tick(object? sender, object e)
        {
            if (_revealStart is { } revealStart)
            {
                if (!_hasRendered)
                {
                    // The first composited frame can land well after RevealAsync was called
                    // (the freshly added overlay still needs to load and lay out); hold the
                    // clock at zero until then so the ripple visibly starts from nothing
                    _revealStart = DateTime.UtcNow;
                }
                else
                {
                    var progress = (float)((DateTime.UtcNow - revealStart).TotalMilliseconds / REVEAL_DURATION_MS);
                    if (progress >= 1f)
                    {
                        _revealProgress = 1f;
                        _revealStart = null;
                        _shadowStart = DateTime.UtcNow;
                        _revealTcs?.TrySetResult();
#if !HAS_UNO_SKIA
                        AttachGpuCanvas();
#endif
                    }
                    else
                    {
                        // Cubic ease-in-out: the ripple builds up gently, sweeps, then settles
                        _revealProgress = progress < 0.5f
                            ? 4f * progress * progress * progress
                            : 1f - MathF.Pow(-2f * progress + 2f, 3f) / 2f;
                    }
                }
            }

            if (_shadowStart is { } shadowStart)
            {
                // Fade the content shadow in alongside the content's own entrance animation
                var progress = (float)((DateTime.UtcNow - shadowStart).TotalMilliseconds / SHADOW_FADE_MS);
                _shadowOpacity = progress >= 1f ? 1f : 1f - MathF.Pow(1f - progress, 3f);
            }

#if HAS_UNO_SKIA
            _canvas.Invalidate();
#else
            _gpuCanvas?.Invalidate();
            if (!_revealCanvasRemoved)
                _revealCanvas.Invalidate();
#endif
        }

        private void PrepareFrame(float shadowScale)
        {
            _hasRendered = true;
            _renderer.ShadowOpacity = _shadowOpacity;
            _renderer.ShadowBounds = _contentBounds is { } bounds
                ? new SKRect(
                    (float)(bounds.X * shadowScale),
                    (float)(bounds.Y * shadowScale),
                    (float)((bounds.X + bounds.Width) * shadowScale),
                    (float)((bounds.Y + bounds.Height) * shadowScale))
                : null;
        }

#if HAS_UNO_SKIA
        private sealed class CompositionCanvas(IntroductionBackground owner) : SKCanvasElement
        {
            protected override void RenderOverride(SKCanvas canvas, Size area)
            {
                owner.PrepareFrame(shadowScale: 1f);
                owner._renderer.Render(
                    canvas,
                    (float)area.Width,
                    (float)area.Height,
                    (float)owner._clock.Elapsed.TotalSeconds,
                    owner._revealProgress);
            }
        }
#else
        private void RenderFrame(SKCanvas canvas, SKImageInfo info)
        {
            // Both canvases work in physical pixels while element bounds are logical
            var scale = ActualWidth > 0 ? (float)(info.Width / ActualWidth) : 1f;
            PrepareFrame(scale);

            canvas.Clear(SKColors.Transparent);
            _renderer.Render(
                canvas,
                info.Width,
                info.Height,
                (float)_clock.Elapsed.TotalSeconds,
                _revealProgress);
        }

        private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            RenderFrame(e.Surface.Canvas, e.Info);
        }

        private void GpuCanvas_PaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
        {
            RenderFrame(e.Surface.Canvas, e.Info);

            // The CPU canvas stays stacked on top until the swap chain has painted, so the
            // handoff never flashes an unpainted (opaque black) panel
            if (!_revealCanvasRemoved)
                DispatcherQueue.TryEnqueue(RemoveRevealCanvas);
        }

        private void AttachGpuCanvas()
        {
            if (_gpuCanvas is not null)
                return;

            _gpuCanvas = new SKSwapChainPanel();
            _gpuCanvas.PaintSurface += GpuCanvas_PaintSurface;
            _gpuCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            _gpuCanvas.VerticalAlignment = VerticalAlignment.Stretch;

            // Inserted behind the CPU canvas: both hosts render identical frames during
            // the handoff, so the swap is not visible
            _canvasHost.Children.Insert(0, _gpuCanvas);

            _renderer.HighQualitySampling = true;
            _frameTimer.Interval = TimeSpan.FromMilliseconds(GPU_FRAME_INTERVAL_MS);
        }

        private void RemoveRevealCanvas()
        {
            if (_revealCanvasRemoved)
                return;

            _revealCanvasRemoved = true;
            _revealCanvas.PaintSurface -= Canvas_PaintSurface;
            _canvasHost.Children.Remove(_revealCanvas);
        }
#endif

        private void IntroductionBackground_Loaded(object sender, RoutedEventArgs e)
        {
            _renderer.SetTheme(ActualTheme == ElementTheme.Light);
            _frameTimer.Start();
        }

        private void IntroductionBackground_Unloaded(object sender, RoutedEventArgs e)
        {
            _frameTimer.Stop();
            _hasRendered = false;

            // Never leave a pending reveal awaiter hanging if the control is torn down mid-animation
            _revealTcs?.TrySetResult();
        }

        private void IntroductionBackground_ActualThemeChanged(FrameworkElement sender, object args)
        {
            _renderer.SetTheme(ActualTheme == ElementTheme.Light);
        }

        /// <inheritdoc/>
        public new void Dispose()
        {
            _frameTimer.Stop();
            _frameTimer.Tick -= FrameTimer_Tick;

#if !HAS_UNO_SKIA
            if (_gpuCanvas is not null)
                _gpuCanvas.PaintSurface -= GpuCanvas_PaintSurface;
            if (!_revealCanvasRemoved)
                _revealCanvas.PaintSurface -= Canvas_PaintSurface;
#endif

            Loaded -= IntroductionBackground_Loaded;
            Unloaded -= IntroductionBackground_Unloaded;
            ActualThemeChanged -= IntroductionBackground_ActualThemeChanged;

            _renderer.Dispose();
        }
    }
}
