using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Diagnostics;
using System.IO.Compression;
using SecureFolderFS.Shared.Helpers;
using SkottieAnimation = SkiaSharp.Skottie.Animation;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class SkottieView : ContentView, IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private SkottieAnimation? _animation;
        private IDispatcherTimer? _timer;
        private bool _isPlaying;

        public SkottieView()
        {
            _stopwatch = new Stopwatch();
            InitializeComponent();
        }

        /// <summary>
        /// Gets the duration of the animation.
        /// </summary>
        public TimeSpan Duration => _animation?.Duration ?? TimeSpan.Zero;

        /// <summary>
        /// Gets whether the animation is currently playing.
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Starts or resumes the animation playback.
        /// </summary>
        public void Play()
        {
            if (_animation is null || _isPlaying)
                return;

            _isPlaying = true;
            _stopwatch.Start();
            StartTimer();
        }

        /// <summary>
        /// Pauses the animation playback.
        /// </summary>
        public void Pause()
        {
            _isPlaying = false;
            _stopwatch.Stop();
            StopTimer();
        }

        /// <summary>
        /// Stops the animation and resets to the beginning.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _stopwatch.Reset();
            
            StopTimer();
            CanvasView.InvalidateSurface();
        }

        /// <inheritdoc/>
        protected override void OnHandlerChanging(HandlerChangingEventArgs args)
        {
            base.OnHandlerChanging(args);

            // If the new handler is null, the view is being disconnected/removed
            if (args.NewHandler is null)
                Dispose();
        }

        private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SkottieView skottieView)
                skottieView.LoadAnimation();
        }

        private async void LoadAnimation()
        {
            // Clean up existing animation
            Stop();
            _animation?.Dispose();
            _animation = null;

            if (string.IsNullOrEmpty(Source))
                return;

            try
            {
                Stream? stream;

                // Try to load from embedded resource or file
                if (Source.StartsWith("resource://"))
                {
                    var resourcePath = Source.Substring("resource://".Length);
                    stream = await SafetyHelpers.NoFailureAsync(async () => await FileSystem.OpenAppPackageFileAsync(resourcePath));
                }
                else if (File.Exists(Source))
                {
                    stream = SafetyHelpers.NoFailureResult(() => File.OpenRead(Source));
                }
                else
                {
                    // Try as MauiAsset (embedded resource)
                    stream = await SafetyHelpers.NoFailureAsync(async () => await FileSystem.OpenAppPackageFileAsync(Source));
                }

                if (stream is null)
                    return;

                await using (stream)
                {
                    // Check if it's a .lottie file (dotLottie format - ZIP archive)
                    if (Source.EndsWith(".lottie", StringComparison.OrdinalIgnoreCase))
                    {
                        await using var jsonStream = await ExtractLottieJsonAsync(stream);
                        if (jsonStream is null)
                            return;

                        if (!SkottieAnimation.TryCreate(jsonStream, out _animation))
                            return;
                    }
                    else
                    {
                        if (!SkottieAnimation.TryCreate(stream, out _animation))
                            return;
                    }

                    if (AutoPlay)
                        Play();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Extracts the animation JSON from a .lottie (dotLottie) ZIP archive.
        /// </summary>
        private static async Task<MemoryStream?> ExtractLottieJsonAsync(Stream lottieStream)
        {
            try
            {
                await using var archive = new ZipArchive(lottieStream, ZipArchiveMode.Read, leaveOpen: true);

                // dotLottie v2 format uses "a" folder for animations
                // dotLottie v1 format uses "animations" folder
                // Some files may have root-level animation.json
                var jsonEntry = archive.GetEntry("a/animation.json")
                                ?? archive.GetEntry("animations/animation.json")
                                ?? archive.GetEntry("animation.json")
                                ?? archive.GetEntry("data.json")
                                ?? archive.Entries.FirstOrDefault(e =>
                                    e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                                    !e.FullName.Contains("manifest", StringComparison.OrdinalIgnoreCase));

                if (jsonEntry is null)
                    return null;

                var memoryStream = new MemoryStream();
                await using (var entryStream = await jsonEntry.OpenAsync())
                    await entryStream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void StartTimer()
        {
            if (_timer is not null)
                return;

            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer is null)
                return;

            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (!_isPlaying)
                return;

            CanvasView.InvalidateSurface();
        }

        private void CanvasView_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_animation is null)
                return;

            var elapsed = _stopwatch.Elapsed;
            var duration = _animation.Duration;
            if (elapsed >= duration)
            {
                if (RepeatAnimation)
                {
                    _stopwatch.Restart();
                    elapsed = TimeSpan.Zero;
                }
                else
                {
                    _isPlaying = false;
                    StopTimer();
                    elapsed = duration;
                }
            }

            // Seek to the current time
            _animation.SeekFrameTime(elapsed.TotalSeconds);

            // Calculate the destination rectangle to fit the animation
            var info = e.Info;
            var animSize = _animation.Size;

            // Scale to fit while maintaining aspect ratio
            var scaleX = info.Width / animSize.Width;
            var scaleY = info.Height / animSize.Height;
            var scale = Math.Min(scaleX, scaleY);

            var destWidth = animSize.Width * scale;
            var destHeight = animSize.Height * scale;
            var destX = (info.Width - destWidth) / 2;
            var destY = (info.Height - destHeight) / 2;

            var destRect = new SKRect(destX, destY, destX + destWidth, destY + destHeight);

            // Render the animation
            _animation.Render(canvas, destRect);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
            _animation?.Dispose();
            _animation = null;
        }

        /// <summary>
        /// Gets or sets the source path of the Lottie animation JSON file.
        /// </summary>
        public string? Source
        {
            get => (string?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(string), typeof(SkottieView), propertyChanged: OnSourceChanged);

        /// <summary>
        /// Gets or sets whether the animation should repeat.
        /// </summary>
        public bool RepeatAnimation
        {
            get => (bool)GetValue(RepeatAnimationProperty);
            set => SetValue(RepeatAnimationProperty, value);
        }
        public static readonly BindableProperty RepeatAnimationProperty =
            BindableProperty.Create(nameof(RepeatAnimation), typeof(bool), typeof(SkottieView), true);

        /// <summary>
        /// Gets or sets whether the animation should auto-play when loaded.
        /// </summary>
        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }
        public static readonly BindableProperty AutoPlayProperty =
            BindableProperty.Create(nameof(AutoPlay), typeof(bool), typeof(SkottieView), true);
    }
}
