using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#if HAS_UNO_SKIA
using Windows.Foundation;
using SkiaSharp;
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
        private const double REVEAL_DURATION_MS = 750d;

#if HAS_UNO_SKIA
        // SKCanvasElement draws directly in the app's composition pass (GPU-backed,
        // no per-frame bitmap upload), so a full frame rate is affordable
        private const int FRAME_INTERVAL_MS = 16;
        private readonly CompositionCanvas _canvas;
#else
        // SKXamlCanvas rasterizes on the CPU and uploads the bitmap each frame - keep
        // the cadence lower and let the renderer use cheaper sampling
        private const int FRAME_INTERVAL_MS = 33;
        private readonly SKXamlCanvas _canvas;
#endif

        private readonly IntroductionBackgroundRenderer _renderer = new();
        private readonly DispatcherTimer _frameTimer;
        private readonly Stopwatch _clock = Stopwatch.StartNew();

        private float _revealProgress;
        private DateTime? _revealStart;
        private TaskCompletionSource? _revealTcs;

        public IntroductionBackground()
        {
            IsHitTestVisible = false;

#if HAS_UNO_SKIA
            _canvas = new CompositionCanvas(this);
#else
            _renderer.HighQualitySampling = false;
            _canvas = new SKXamlCanvas();
            _canvas.PaintSurface += Canvas_PaintSurface;
#endif
            _canvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            _canvas.VerticalAlignment = VerticalAlignment.Stretch;
            Content = _canvas;

            _frameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FRAME_INTERVAL_MS) };
            _frameTimer.Tick += FrameTimer_Tick;

            Loaded += IntroductionBackground_Loaded;
            Unloaded += IntroductionBackground_Unloaded;
            ActualThemeChanged += IntroductionBackground_ActualThemeChanged;
        }

        /// <summary>
        /// Sweeps the background into view with a bottom-up gradient wipe.
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

        private void FrameTimer_Tick(object? sender, object e)
        {
            if (_revealStart is { } revealStart)
            {
                var progress = (float)((DateTime.UtcNow - revealStart).TotalMilliseconds / REVEAL_DURATION_MS);
                if (progress >= 1f)
                {
                    _revealProgress = 1f;
                    _revealStart = null;
                    _revealTcs?.TrySetResult();
                }
                else
                    _revealProgress = 1f - MathF.Pow(1f - progress, 3f); // ease-out cubic
            }

            _canvas.Invalidate();
        }

#if HAS_UNO_SKIA
        private sealed class CompositionCanvas(IntroductionBackground owner) : SKCanvasElement
        {
            protected override void RenderOverride(SKCanvas canvas, Size area)
            {
                owner._renderer.Render(
                    canvas,
                    (float)area.Width,
                    (float)area.Height,
                    (float)owner._clock.Elapsed.TotalSeconds,
                    owner._revealProgress);
            }
        }
#else
        private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SkiaSharp.SKColors.Transparent);
            _renderer.Render(
                e.Surface.Canvas,
                e.Info.Width,
                e.Info.Height,
                (float)_clock.Elapsed.TotalSeconds,
                _revealProgress);
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

            Loaded -= IntroductionBackground_Loaded;
            Unloaded -= IntroductionBackground_Unloaded;
            ActualThemeChanged -= IntroductionBackground_ActualThemeChanged;

            _renderer.Dispose();
        }
    }
}
