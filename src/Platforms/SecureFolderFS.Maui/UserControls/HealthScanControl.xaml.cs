using CommunityToolkit.Maui.Core;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class HealthScanControl : ContentView, IDisposable
    {
        private CancellationTokenSource? _shimmerCts;

        public event EventHandler? Clicked;

        public HealthScanControl()
        {
            InitializeComponent();
            GradientBackground.Background = GetGradientBrush(Severity);
        }

        private void StartShimmer()
        {
            _shimmerCts?.Cancel();
            _shimmerCts = new CancellationTokenSource();
            var token = _shimmerCts.Token;

            ShimmerOverlay.IsVisible = true;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        var cardWidth = CardBorder.Width;
                        ShimmerOverlay.TranslationX = -cardWidth;
                        await ShimmerOverlay.TranslateToAsync(cardWidth, 0, 900, Easing.SinInOut);
                    });
                    await Task.Delay(400, token).ContinueWith(_ => { }, CancellationToken.None);
                }
            }, token);
        }

        private void StopShimmer()
        {
            _shimmerCts?.Cancel();
            _shimmerCts = null;
            ShimmerOverlay.IsVisible = false;
            ShimmerOverlay.CancelAnimations();
        }

        private static LinearGradientBrush GetGradientBrush(Severity severity)
        {
            var (start, end) = severity switch
            {
                Severity.Success => (Color.FromArgb("#2A7A50"), Color.FromArgb("#3DB874")),
                Severity.Warning => (Color.FromArgb("#A05C00"), Color.FromArgb("#D4860A")),
                Severity.Critical => (Color.FromArgb("#8B1F30"), Color.FromArgb("#C94055")),
                _ => MauiThemeHelper.Instance.ActualTheme switch
                {
                    ThemeType.Light => (Color.FromArgb("#6B8BA4"), Color.FromArgb("#8AAEC7")),
                    _ => (Color.FromArgb("#3A3E45"), Color.FromArgb("#52575F"))
                }
            };

            return new LinearGradientBrush([
                    new GradientStop(start, 0f),
                    new GradientStop(end, 1f)
                ],
                new Point(0, 0.5),
                new Point(1, 0.5));
        }

        private void TouchBehavior_TouchGestureCompleted(object? sender, TouchGestureCompletedEventArgs e)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        public Severity Severity
        {
            get => (Severity)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }
        public static readonly BindableProperty SeverityProperty =
            BindableProperty.Create(nameof(Severity), typeof(Severity), typeof(HealthScanControl), Severity.Default,
                propertyChanged: static (bindable, _, newValue) =>
                {
                    if (bindable is HealthScanControl control && newValue is Severity severity)
                        control.GradientBackground.Background = GetGradientBrush(severity);
                });

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(HealthScanControl));

        public string? Subtitle
        {
            get => (string?)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(HealthScanControl));

        public object? IconSlot
        {
            get => (object?)GetValue(IconSlotProperty);
            set => SetValue(IconSlotProperty, value);
        }
        public static readonly BindableProperty IconSlotProperty =
            BindableProperty.Create(nameof(IconSlot), typeof(object), typeof(HealthScanControl));

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(HealthScanControl), false,
                propertyChanged: static (bindable, _, newValue) =>
                {
                    if (bindable is not HealthScanControl control)
                        return;

                    if (newValue is true)
                        control.StartShimmer();
                    else
                        control.StopShimmer();
                });

        /// <inheritdoc/>
        public void Dispose()
        {
            StopShimmer();
        }
    }
}
