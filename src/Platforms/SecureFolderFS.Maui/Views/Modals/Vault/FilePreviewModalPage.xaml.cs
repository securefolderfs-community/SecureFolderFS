using LibVLCSharp.MAUI;
using LibVLCSharp.Shared;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.TemplateSelectors;
using SecureFolderFS.Maui.UserControls;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using Page = Microsoft.Maui.Controls.Page;
#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
#endif

namespace SecureFolderFS.Maui.Views.Modals.Vault
{
    public partial class FilePreviewModalPage : BaseModalPage, IOverlayControl
    {
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;

        public FilePreviewModalPage(INavigation sourceNavigation)
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
            navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);
            await _sourceNavigation.PushModalAsync(navigationPage);
#endif

            return await _modalTcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = viewable as PreviewerOverlayViewModel;
            OnPropertyChanged(nameof(ViewModel));
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            _modalTcs.TrySetResult(Result.Success);
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _modalTcs.TrySetResult(Result.Success);
            if (GalleryView is not null)
            {
                GalleryView.PreviousRequested -= Gallery_PreviousRequested;
                GalleryView.NextRequested -= Gallery_NextRequested;
                GalleryView.DismissRequested -= Gallery_DismissRequested;
                GalleryView.Dispose();

                (GalleryView.Previous as IDisposable)?.Dispose();
                (GalleryView.Current as IDisposable)?.Dispose();
                (GalleryView.Next as IDisposable)?.Dispose();
            }
        }

        private View? CreateGalleryView(CarouselPreviewerViewModel carouselViewModel, int index)
        {
            var viewModel = carouselViewModel.Slides.ElementAtOrDefault(index);
            if (viewModel is null)
                return null;

            var presentation = new ContentPresentation()
            {
                Presentation = viewModel,
                TemplateSelector = new PreviewerTemplateSelector()
                {
                    ImageTemplate = Resources["ImageTemplate"] as DataTemplate,
                    VideoTemplate = Resources["VideoTemplate"] as DataTemplate
                }
            };
            presentation.Loaded += Presentation_Loaded;
            return presentation;

            void Presentation_Loaded(object? sender, EventArgs e)
            {
                presentation.Loaded -= Presentation_Loaded;
                var descendants = presentation.GetVisualTreeDescendants();
                var found = descendants.FirstOrDefault(x => x is PanPinchContainer or PanRouter);

                switch (found)
                {
                    case PanPinchContainer panPinchContainer:
                    {
                        panPinchContainer.TappedCommand ??= ViewModel?.ToggleImmersionCommand;
                        panPinchContainer.PanUpdatedCommand ??= GalleryView?.PanUpdatedCommand;
                        break;
                    }
                    
                    case PanRouter panRouter:
                    {
                        panRouter.TappedCommand ??= ViewModel?.ToggleImmersionCommand;
                        panRouter.PanUpdatedCommand ??= GalleryView?.PanUpdatedCommand;
                        break;
                    }
                }
            }
        }

        private async void Closed_Clicked(object? sender, EventArgs e)
        {
            await HideAsync();
        }

        private void MediaPlayerElement_Loaded(object? sender, EventArgs e)
        {
            if (sender is not MediaPlayerElement { BindingContext: VideoPreviewerViewModel videoViewModel } mediaPlayerElement)
                return;

            if (videoViewModel.VideoSource is not ICollection<IDisposable> disposables || disposables.ElementAtOrDefault(0) is not Stream stream)
                return;

            var existingMediaPlayer = disposables.OfType<LibVLCSharp.Shared.MediaPlayer>().FirstOrDefault();
            var existingLibVlc = disposables.OfType<LibVLC>().FirstOrDefault();
            if (existingMediaPlayer is not null && existingLibVlc is not null)
            {
                mediaPlayerElement.LibVLC = existingLibVlc;
                mediaPlayerElement.MediaPlayer = existingMediaPlayer;

                if (!existingMediaPlayer.IsPlaying)
                    existingMediaPlayer.Play();

                return;
            }

            if (stream.CanSeek)
                stream.Position = 0L;

            var libVlc = new LibVLC("--input-repeat=65545");
            var mediaInput = new StreamMediaInput(stream);
            var media = new Media(libVlc, mediaInput);
            media.AddOption(":input-repeat=65545");
            var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVlc)
            {
                Media = media
            };

            disposables.Add(libVlc);
            disposables.Add(mediaInput);
            disposables.Add(media);
            disposables.Add(mediaPlayer);

            mediaPlayerElement.LibVLC = libVlc;
            mediaPlayerElement.MediaPlayer = mediaPlayer;
            mediaPlayer.Play();
        }

        private void GalleryView_Loaded(object? sender, EventArgs e)
        {
            if (GalleryView is not null)
                return;

            if (sender is not GalleryView { BindingContext: CarouselPreviewerViewModel carouselViewModel } galleryView)
                return;

            GalleryView = galleryView;
            galleryView.FitToParent();

            galleryView.PreviousRequested += Gallery_PreviousRequested;
            galleryView.NextRequested += Gallery_NextRequested;
            galleryView.DismissRequested += Gallery_DismissRequested;

            (carouselViewModel.Slides.ElementAtOrDefault(carouselViewModel.CurrentIndex - 1) as IAsyncInitialize)?.InitAsync();
            (carouselViewModel.Slides.ElementAtOrDefault(carouselViewModel.CurrentIndex) as IAsyncInitialize)?.InitAsync();
            (carouselViewModel.Slides.ElementAtOrDefault(carouselViewModel.CurrentIndex + 1) as IAsyncInitialize)?.InitAsync();
            (carouselViewModel.Slides.ElementAtOrDefault(carouselViewModel.CurrentIndex) as IViewDesignation)?.OnAppearing();

            galleryView.Previous = carouselViewModel.CurrentIndex > 0 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex - 1) : null;
            galleryView.Current = CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex);
            galleryView.Next = carouselViewModel.CurrentIndex < carouselViewModel.Slides.Count - 1 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex + 1) : null;
            galleryView.RefreshLayout();

            carouselViewModel.Title = carouselViewModel.Slides.ElementAtOrDefault(carouselViewModel.CurrentIndex)?.Title;
        }

        private void Gallery_PreviousRequested(object? sender, EventArgs e)
        {
            if (sender is not GalleryView { BindingContext: CarouselPreviewerViewModel carouselViewModel } galleryView)
                return;

            if (carouselViewModel.CurrentIndex <= 0)
                return;

            var oldIndex = carouselViewModel.CurrentIndex;
            var oldNext = carouselViewModel.Slides.ElementAtOrDefault(oldIndex + 1);
            var oldCurrent = carouselViewModel.Slides.ElementAtOrDefault(oldIndex);
            (oldNext as IDisposable)?.Dispose();
            (oldCurrent as IViewDesignation)?.OnDisappearing();

            carouselViewModel.CurrentIndex--;
            var newIndex = carouselViewModel.CurrentIndex;
            var newPrevious = carouselViewModel.Slides.ElementAtOrDefault(newIndex - 1);
            var newCurrent = carouselViewModel.Slides.ElementAtOrDefault(newIndex);
            (newPrevious as IAsyncInitialize)?.InitAsync();
            (newCurrent as IViewDesignation)?.OnAppearing();

            galleryView.Previous = carouselViewModel.CurrentIndex > 0 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex - 1) : null;
            galleryView.RefreshLayout();
        }

        private void Gallery_NextRequested(object? sender, EventArgs e)
        {
            if (sender is not GalleryView { BindingContext: CarouselPreviewerViewModel carouselViewModel } galleryView)
                return;

            if (carouselViewModel.CurrentIndex >= carouselViewModel.Slides.Count - 1)
                return;

            var oldIndex = carouselViewModel.CurrentIndex;
            var oldPrevious = carouselViewModel.Slides.ElementAtOrDefault(oldIndex - 1);
            var oldCurrent = carouselViewModel.Slides.ElementAtOrDefault(oldIndex);
            (oldPrevious as IDisposable)?.Dispose();
            (oldCurrent as IViewDesignation)?.OnDisappearing();

            carouselViewModel.CurrentIndex++;
            var newIndex = carouselViewModel.CurrentIndex;
            var newNext = carouselViewModel.Slides.ElementAtOrDefault(newIndex + 1);
            var newCurrent = carouselViewModel.Slides.ElementAtOrDefault(newIndex);
            (newNext as IAsyncInitialize)?.InitAsync();
            (newCurrent as IViewDesignation)?.OnAppearing();

            galleryView.Next = carouselViewModel.CurrentIndex < carouselViewModel.Slides.Count - 1 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex + 1) : null;
            galleryView.RefreshLayout();
        }
        
        private async void Gallery_DismissRequested(object? sender, EventArgs e)
        {
            // The gallery has already translated/faded partway - animate the rest out, then dismiss
            var page = this as Page;
            var slideTask = page.TranslateToAsync(0, 60, 180U, Easing.CubicIn);
            var fadeTask = page.FadeToAsync(0d, 180U, Easing.CubicIn);

            await Task.WhenAll(slideTask, fadeTask);
            await HideAsync();
        }
        
        public GalleryView? GalleryView
        {
            get => (GalleryView?)GetValue(GalleryViewProperty);
            private set => SetValue(GalleryViewProperty, value);
        }
        public static readonly BindableProperty GalleryViewProperty =
            BindableProperty.Create(nameof(GalleryView), typeof(GalleryView), typeof(FilePreviewModalPage));

        public PreviewerOverlayViewModel? ViewModel
        {
            get => (PreviewerOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PreviewerOverlayViewModel), typeof(FilePreviewModalPage));
    }
}

