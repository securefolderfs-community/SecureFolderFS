using CommunityToolkit.Maui.Core;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class HealthPage : ContentPage, IQueryAttributable
    {
        private CancellationTokenSource? _shimmerCts;
        
        public HealthPage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Unsubscribe from previous
            ViewModel?.HealthViewModel.PropertyChanged -= HealthViewModel_PropertyChanged;
            
            ViewModel = query.ToViewModel<VaultHealthReportViewModel>();
            OnPropertyChanged(nameof(ViewModel));

            ViewModel?.HealthViewModel.PropertyChanged += HealthViewModel_PropertyChanged;
            GradientBackground.Background = GetGradientBrush(ViewModel?.HealthViewModel.Severity ?? Severity.Default);
            
            if (ViewModel?.HealthViewModel.IsProgressing ?? false)
                StartShimmer();
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel?.HealthViewModel.PropertyChanged -= HealthViewModel_PropertyChanged;
            _shimmerCts?.Dispose();
        }

        public static Brush GetGradientBrush(Severity severity)
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
        
        private void StartShimmer()
        {
            _shimmerCts?.Cancel();
            _shimmerCts = new CancellationTokenSource();
            var token = _shimmerCts.Token;

            ShimmerOverlay.IsVisible = true;

            // Reset position to far left (off-screen)
            ShimmerOverlay.TranslationX = -StatusCard.Width;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        ShimmerOverlay.TranslationX = -StatusCard.Width;
                        await ShimmerOverlay.TranslateToAsync(StatusCard.Width, 0, 900, Easing.SinInOut);
                    });
                    await Task.Delay(400, token).ContinueWith(_ => { }, CancellationToken.None); // pause between sweeps
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
        
        private void StatusCard_TouchGestureCompleted(object? sender, TouchGestureCompletedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (ViewModel.HealthViewModel.IsProgressing)
                ViewModel.HealthViewModel.CancelScanningCommand.Execute(null);
            else
                ViewModel.HealthViewModel.StartScanningCommand.Execute(null);
        }
        
        private void HealthViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not VaultHealthViewModel viewModel)
                return;

            switch (e.PropertyName)
            {
                case nameof(viewModel.Severity):
                    GradientBackground.Background = GetGradientBrush(viewModel.Severity);
                    break;
                
                case nameof(viewModel.IsProgressing) when viewModel.IsProgressing:
                    StartShimmer();
                    break;
                
                case nameof(viewModel.IsProgressing) when !viewModel.IsProgressing:
                    StopShimmer();
                    break;
            }
        }

        public VaultHealthReportViewModel? ViewModel
        {
            get => (VaultHealthReportViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultHealthReportViewModel), typeof(HealthPage));
    }
}
