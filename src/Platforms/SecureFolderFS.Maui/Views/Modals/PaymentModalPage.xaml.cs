using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
#endif

namespace SecureFolderFS.Maui.Views.Modals
{
    public partial class PaymentModalPage : BaseModalPage, IOverlayControl
    {
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;
        private bool _isPulseAnimationRunning;
        private bool _stopPulseAnimation;

        public PaymentModalPage(INavigation sourceNavigation)
        {
            _sourceNavigation = sourceNavigation;
            _modalTcs = new();
            BindingContext = this;

            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            // Using Shell to display modals is broken - each new page shown after a 'modal' page will be incorrectly displayed as another modal.
            // NavigationPage approach does not have this issue
#if ANDROID
            await _sourceNavigation.PushModalAsync(new NavigationPage(this)
            {
                BackgroundColor = Color.FromArgb("#80000000")
            });
#elif IOS
            var navigationPage = new NavigationPage(this);
            NavigationPage.SetIconColor(navigationPage, Color.FromArgb("#007bff"));
            navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
            await _sourceNavigation.PushModalAsync(navigationPage);
#endif

            return await _modalTcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = viewable as PaymentOverlayViewModel;
            OnPropertyChanged(nameof(ViewModel));
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _stopPulseAnimation = false;
            _ = StartRingPulseAnimationAsync();
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            StopRingPulseAnimation();
            base.OnDisappearing();
            _modalTcs.TrySetResult(Result.Success);
        }

        private async Task StartRingPulseAnimationAsync()
        {
            if (_isPulseAnimationRunning)
                return;

            _isPulseAnimationRunning = true;

            try
            {
                while (!_stopPulseAnimation && IsVisible)
                {
                    var outerGrowTask = OuterRing.ScaleToAsync(1.08, 2000, Easing.SinInOut);
                    var innerGrowTask = InnerRing.ScaleToAsync(1.10, 2000, Easing.SinInOut);
                    await Task.WhenAll(outerGrowTask, innerGrowTask);

                    if (_stopPulseAnimation || !IsVisible)
                        break;

                    var outerShrinkTask = OuterRing.ScaleToAsync(1.0, 2000, Easing.SinInOut);
                    var innerShrinkTask = InnerRing.ScaleToAsync(1.0, 2000, Easing.SinInOut);
                    await Task.WhenAll(outerShrinkTask, innerShrinkTask);
                }
            }
            finally
            {
                _isPulseAnimationRunning = false;
            }
        }

        private void StopRingPulseAnimation()
        {
            _stopPulseAnimation = true;
            OuterRing.CancelAnimations();
            InnerRing.CancelAnimations();
            OuterRing.Scale = 1.0;
            InnerRing.Scale = 1.0;
        }

        public PaymentOverlayViewModel? ViewModel
        {
            get => (PaymentOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PaymentOverlayViewModel), typeof(PaymentModalPage));
    }
}
