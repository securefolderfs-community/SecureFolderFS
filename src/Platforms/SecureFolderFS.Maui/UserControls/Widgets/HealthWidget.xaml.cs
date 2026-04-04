using System.Windows.Input;
using SecureFolderFS.Maui.Views.Vault;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Maui.UserControls.Widgets
{
    public partial class HealthWidget : ContentView, IDisposable
    {
        private CancellationTokenSource? _shimmerCts;
        
        public HealthWidget()
        {
            InitializeComponent();
            GradientBackground.Background = HealthPage.GetGradientBrush(Severity);
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

        public Severity Severity
        {
            get => (Severity)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }
        public static readonly BindableProperty SeverityProperty =
            BindableProperty.Create(nameof(Severity), typeof(Severity), typeof(HealthWidget), Severity.Default,
                propertyChanged: static (bindable, _, newValue) =>
                {
                    if (bindable is HealthWidget widget && newValue is Severity severity)
                        widget.GradientBackground.Background = HealthPage.GetGradientBrush(severity);
                });

        public string? StatusTitle
        {
            get => (string?)GetValue(StatusTitleProperty);
            set => SetValue(StatusTitleProperty, value);
        }
        public static readonly BindableProperty StatusTitleProperty =
            BindableProperty.Create(nameof(StatusTitle), typeof(string), typeof(HealthWidget));

        public string? LastCheckedText
        {
            get => (string?)GetValue(LastCheckedTextProperty);
            set => SetValue(LastCheckedTextProperty, value);
        }
        public static readonly BindableProperty LastCheckedTextProperty =
            BindableProperty.Create(nameof(LastCheckedText), typeof(string), typeof(HealthWidget));

        public ICommand? OpenVaultHealthCommand
        {
            get => (ICommand?)GetValue(OpenVaultHealthCommandProperty);
            set => SetValue(OpenVaultHealthCommandProperty, value);
        }
        public static readonly BindableProperty OpenVaultHealthCommandProperty =
            BindableProperty.Create(nameof(OpenVaultHealthCommand), typeof(ICommand), typeof(HealthWidget));
        
        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(HealthWidget), false,
                propertyChanged: static (bindable, _, newValue) =>
                {
                    if (bindable is not HealthWidget widget)
                        return;

                    if (newValue is true)
                        widget.StartShimmer();
                    else
                        widget.StopShimmer();
                });

        /// <inheritdoc/>
        public void Dispose()
        {
            StopShimmer();
        }
    }
}

