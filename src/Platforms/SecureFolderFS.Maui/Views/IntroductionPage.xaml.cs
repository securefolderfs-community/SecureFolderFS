using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using Application = Microsoft.Maui.Controls.Application;

namespace SecureFolderFS.Maui.Views
{
    public partial class IntroductionPage : ContentPage, IOverlayControl
    {
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;
        private int _currentIndex;
        private GalleryView? _galleryView;
        private Button? _continueButton;

        public IntroductionPage(INavigation sourceNavigation)
        {
            _sourceNavigation = sourceNavigation;
            _modalTcs = new();
            BindingContext = this;

            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            await _sourceNavigation.PushModalAsync(this);
            return await _modalTcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            _ = viewable;
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _modalTcs.TrySetResult(Result.Success);
        }

        /// <inheritdoc/>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(400);

            BackgroundView.TranslationY = 800d;
            _ = BackgroundView.FadeToAsync(1, 800U, Easing.CubicIn);
            _ = BackgroundView.TranslateToAsync(0, -300, 3000U, EasingHelpers.EaseOutExpo);
            
#if ANDROID
            _ = Task.Delay(900).ContinueWith(async _ => await SystemNavBarGradient.FadeToAsync(1, 300U, Easing.CubicIn));
#endif
            
            await Task.Delay(600);
#if ANDROID
            _ = ForegroundView.FadeToAsync(1, 600U, Easing.CubicIn);
#elif IOS || MACCATALYST
            _ = BlurredForegroundView.FadeToAsync(1, 600U, Easing.CubicIn);
#endif
        }

        private void UpdateButtonStyle()
        {
            if (_currentIndex >= 3)
            {
                _continueButton?.Style = Application.Current?.Resources.Get("AccentButtonStyle") as Style;
                _continueButton?.Text = "Done".ToLocalized();
            }
            else
            {
                _continueButton?.Style = Application.Current?.Resources.Get("DefaultButtonStyle") as Style;
                _continueButton?.Text = "Continue".ToLocalized();
            }
        }

        private void GalleryView_Loaded(object? sender, EventArgs e)
        {
            if (sender is not GalleryView galleryView)
                return;
            
            _galleryView = galleryView;
            galleryView.NextRequested += GalleryView_NextRequested;
            galleryView.PreviousRequested += GalleryView_PreviousRequested;

            galleryView.Current = Resources.Get("Slide0") as View;
            galleryView.Next = Resources.Get("Slide1") as View;
            galleryView.RefreshLayout();
        }

        private void GalleryView_PreviousRequested(object? sender, EventArgs e)
        {
            if (sender is not GalleryView galleryView)
                return;

            _currentIndex -= 1;
            galleryView.Previous = Resources.Get($"Slide{_currentIndex - 1}") as View;
            galleryView.RefreshLayout();
            UpdateButtonStyle();
        }

        private void GalleryView_NextRequested(object? sender, EventArgs e)
        {
            if (sender is not GalleryView galleryView)
                return;

            _currentIndex += 1;
            galleryView.Next = Resources.Get($"Slide{_currentIndex + 1}") as View;
            galleryView.RefreshLayout();
            UpdateButtonStyle();
        }

        private async void Continue_Clicked(object? sender, EventArgs e)
        {
            if (_galleryView is null)
                return;
            
            if (_currentIndex >= 3)
            {
                await MainPage.Instance!.Navigation.PopAsync();
                return;
            }

            await _galleryView.SwipeToNextAsync();
        }

        private void PrivacyPolicy_Tapped(object? sender, TappedEventArgs e)
        {
            var applicationService = DI.Service<IApplicationService>();
            applicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/blob/master/PRIVACY.md"));
        }

        private void TermsOfService_Tapped(object? sender, TappedEventArgs e)
        {
            var applicationService = DI.Service<IApplicationService>();
            applicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/blob/master/TERMS_OF_SERVICE.md"));
        }

        private void Continue_Loaded(object? sender, EventArgs e)
        {
            _continueButton = sender as Button;
        }
    }
}

