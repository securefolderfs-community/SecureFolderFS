using SecureFolderFS.Maui.UserControls.Common;
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

        private void GalleryView_Loaded(object? sender, EventArgs e)
        {
            if (sender is not GalleryView galleryView)
                return;
            
            galleryView.FitToParent(BlurView);
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
        }

        private void GalleryView_NextRequested(object? sender, EventArgs e)
        {
            if (sender is not GalleryView galleryView)
                return;

            _currentIndex += 1;
            galleryView.Next = Resources.Get($"Slide{_currentIndex + 1}") as View;
            galleryView.RefreshLayout();
        }
        
        private async void Close_Clicked(object? sender, EventArgs e)
        {
            await MainPage.Instance!.Navigation.PopAsync();
        }
    }
}

