using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace SecureFolderFS.Maui.Views.Modals.DeviceLink
{
    public partial class DeviceLinkRequestPage : BaseModalPage, IOverlayControl
    {
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;
        private bool _isAnimating;
        
        public DeviceLinkRequestPage(INavigation sourceNavigation)
        {
            _sourceNavigation = sourceNavigation;
            _modalTcs = new();
            BindingContext = this;
            InitializeComponent();
        }
        
        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
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
            ViewModel = viewable as DeviceLinkRequestOverlayViewModel;
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
            _ = StartIdleAnimationAsync();
        }
        
        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _modalTcs.TrySetResult(Result.Success);
        }
        
        private async Task StartIdleAnimationAsync()
        {
            // Subtle pulse animation for the key indicator
            while (!_isAnimating && IsVisible)
            {
                await KeyPulseRing.ScaleToAsync(1.2, 1000, Easing.SinInOut);
                await KeyPulseRing.ScaleToAsync(1.0, 1000, Easing.SinInOut);
            }
        }
        
        [RelayCommand]
        private async Task AcceptRequestAsync(CancellationToken cancellationToken)
        {
            if (ViewModel is null)
                return;
            
            if (_isAnimating)
                return;
            
            _isAnimating = true;
            
            // Disable buttons during animation
            AcceptButton.IsEnabled = false;
            RejectButton.IsEnabled = false;
            
            // Phase 1: Key pulses and becomes more visible
            await KeyTransferIndicator.FadeToAsync(1, 150, Easing.CubicIn);
            
            // Phase 2: Key transfer animation - key moves left towards desktop
            _ = KeyPulseRing.ScaleToAsync(1.5, 200, Easing.CubicOut);
            await KeyPulseRing.FadeToAsync(0, 200, Easing.CubicIn);
            
            // Move left towards the desktop (negative X translation)
            await KeyTransferIndicator.TranslateToAsync(-120, 0, 400, Easing.CubicInOut);
            
            // Notify the ViewModel to proceed with the authentication flow in parallel with the animation
            await ViewModel.TryContinueAsync(cancellationToken);
            
            // Phase 3: Success flash on desktop device
            _ = DesktopDevice.ScaleToAsync(1.1, 150, Easing.CubicOut);
            await Task.Delay(100);
            await DesktopDevice.ScaleToAsync(1.0, 150, Easing.CubicIn);
            
            // Phase 4: Fade out entire animation area
            await DeviceAnimationContainer.FadeToAsync(0, 200, Easing.CubicIn);
            
            await Task.Delay(200);
            await HideAsync();
        }

        [RelayCommand]
        private async Task RejectRequestAsync(CancellationToken cancellationToken)
        {
            if (ViewModel is null)
                return;
            
            if (_isAnimating)
                return;
            
            _isAnimating = true;
            
            // Disable buttons during animation
            AcceptButton.IsEnabled = false;
            RejectButton.IsEnabled = false;
            
            // Notify the ViewModel to reject the authentication flow in parallel with the animation
            await ViewModel.TryCancelAsync(cancellationToken);
            
            // Phase 1: Key indicator fades out and shows rejection
            _ = KeyTransferIndicator.FadeToAsync(0, 200, Easing.CubicIn);
            
            // Phase 2: Devices move apart with shake effect
            await DesktopDevice.TranslateToAsync(-10, 0, 50, Easing.Linear);
            await DesktopDevice.TranslateToAsync(10, 0, 50, Easing.Linear);
            await DesktopDevice.TranslateToAsync(-10, 0, 50, Easing.Linear);
            
            // Start parallel animations
            var desktopMoveTask = DesktopDevice.TranslateToAsync(-120, 0, 350, Easing.CubicIn);
            var desktopFadeTask = DesktopDevice.FadeToAsync(0, 350, Easing.CubicIn);
            
            // Mobile stays firm with pulse effect
            var mobilePulseTask = AnimateMobilePulseAsync();
            await Task.WhenAll(desktopMoveTask, desktopFadeTask, mobilePulseTask);
            
            // Phase 3: Brief pause to let user see the rejection
            await Task.Delay(300);
            
            // Phase 4: Fade out
            await DeviceAnimationContainer.FadeToAsync(0, 200, Easing.CubicIn);
            
            await Task.Delay(200);
            await HideAsync();
        }
        
        private async Task AnimateMobilePulseAsync()
        {
            await MobileDevice.ScaleToAsync(1.05, 150, Easing.CubicOut);
            await MobileDevice.ScaleToAsync(1.0, 150, Easing.CubicIn);
        }
        
        public DeviceLinkRequestOverlayViewModel? ViewModel
        {
            get => (DeviceLinkRequestOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(DeviceLinkRequestOverlayViewModel), typeof(DeviceLinkCredentialsPage));
    }
}
