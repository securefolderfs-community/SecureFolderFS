using LibVLCSharp.MAUI;
using LibVLCSharp.Shared;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.TemplateSelectors;
using SecureFolderFS.Maui.UserControls;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
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

        public GalleryView? GalleryView { get; private set; }

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
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            _modalTcs.TrySetResult(Result.Success);
            if (GalleryView is not null)
            {
                GalleryView.PreviousRequested -= Gallery_PreviousRequested;
                GalleryView.NextRequested -= Gallery_NextRequested;
                GalleryView.Dispose();

                (GalleryView.Previous as IDisposable)?.Dispose();
                (GalleryView.Current as IDisposable)?.Dispose();
                (GalleryView.Next as IDisposable)?.Dispose();
            }

            if (ViewModel?.PreviewerViewModel is TextPreviewerViewModel { WasModified: true } textViewModel)
            {
                var overlayService = DI.Service<IOverlayService>();
                var messageOverlay = new MessageOverlayViewModel()
                {
                    Title = "UnsavedChanges".ToLocalized(),
                    Message = "UnsavedChangesDescription".ToLocalized(),
                    PrimaryText = "Save".ToLocalized(),
                    SecondaryText = "Cancel".ToLocalized()
                };

#if IOS
                await Task.Delay(600);
#endif
                var result = await overlayService.ShowAsync(messageOverlay);
                if (result.Positive())
                    await textViewModel.TrySaveAsync();
            }
        }

        private View? CreateGalleryView(CarouselPreviewerViewModel carouselViewModel, int index)
        {
            var viewModel = carouselViewModel.Slides.ElementAtOrDefault(index);
            if (viewModel is null)
                return null;

            return new ContentPresentation()
            {
                Presentation = viewModel,
                TemplateSelector = new PreviewerTemplateSelector()
                {
                    ImageTemplate = Resources["ImageTemplate"] as DataTemplate,
                    VideoTemplate = Resources["VideoTemplate"] as DataTemplate
                }
            };
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

            var libVlc = new LibVLC("--input-repeat=2");
            var mediaInput = new StreamMediaInput(stream);
            var media = new Media(libVlc, mediaInput);
            var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVlc)
            {
                Media = media
            };

            disposables.Add(libVlc);
            disposables.Add(mediaInput);
            disposables.Add(media);
            disposables.Add(mediaPlayer);

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

        public PreviewerOverlayViewModel? ViewModel
        {
            get => (PreviewerOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PreviewerOverlayViewModel), typeof(FilePreviewModalPage));
    }
}

