using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Maui.Views
{
    public partial class IntroductionPage : ContentPage
    {
        private int _currentIndex;
        
        public IntroductionPage()
        {
            InitializeComponent();
        }
        
        /// <inheritdoc/>
        protected override async void OnAppearing()
        {
            await Task.Delay(400);

            BackgroundView.TranslationY = 800d;
            _ = ForegroundView.FadeTo(1, 300U, Easing.CubicIn);
            _ = BackgroundView.FadeTo(1, 800U, Easing.CubicIn);
            await BackgroundView.TranslateTo(0, 0, 3000U, EasingHelpers.EaseOutExpo);

            base.OnAppearing();
        }

        private void UpdateButtonStyle()
        {
            if (_currentIndex >= 3)
            {
                Continue.Style = Application.Current?.Resources.Get("AccentButtonStyle") as Style;
                Continue.Text = "Done".ToLocalized();
            }
            else
            {
                Continue.Style = Application.Current?.Resources.Get("DefaultButtonStyle") as Style;
                Continue.Text = "Continue".ToLocalized();
            }
        }

        private void GalleryView_Loaded(object? sender, EventArgs e)
        {
            GalleryView.NextRequested += GalleryView_NextRequested;
            GalleryView.PreviousRequested += GalleryView_PreviousRequested;

            GalleryView.Current = Resources.Get("Slide0") as View;
            GalleryView.Next = Resources.Get("Slide1") as View;
            GalleryView.RefreshLayout();
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
            if (_currentIndex >= 3)
            {
                await MainPage.Instance!.Navigation.PopAsync();
                return;
            }

            await GalleryView.SwipeToNextAsync();
        }
    }
}

