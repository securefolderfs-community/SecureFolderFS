using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui.Views;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.TemplateSelectors;
using SecureFolderFS.Maui.UserControls;
using SecureFolderFS.Maui.UserControls.Common;
using SecureFolderFS.Maui.ValueConverters;
using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
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
        private GalleryView? _galleryView;

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
            navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
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
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _modalTcs.TrySetResult(Result.Success);
            if ((Presentation.Content as ContentView)?.Content is not MediaElement mediaElement)
                return;

            mediaElement.Stop();
            mediaElement.Handler?.DisconnectHandler();
            mediaElement.Dispose();
            mediaElement.Source = null;
        }

        public PreviewerOverlayViewModel? ViewModel
        {
            get => (PreviewerOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PreviewerOverlayViewModel), typeof(FilePreviewModalPage), null);

        private void Presentation_Loaded(object? sender, EventArgs e)
        {
            if (sender is not ContentPresentation presentation)
                return;

            _ = presentation;
        }
        
        private void Gallery_Loaded(object? sender, EventArgs e)
        {
            if (_galleryView is not null)
                return;

            if (sender is not GalleryView { BindingContext: CarouselPreviewerViewModel carouselViewModel } galleryView)
                return;

            _galleryView = galleryView;
            
            // var parent = galleryView.Parent as ContentView;
            // var height = parent?.Height ?? 300d;
            // var width = parent?.Width ?? 400d;
            //
            // galleryView.WidthRequest = width;
            // galleryView.HeightRequest = height;
            
            galleryView.PreviousRequested += Gallery_PreviousRequested;
            galleryView.NextRequested += Gallery_NextRequested;
            
            var collection = carouselViewModel.Slides;
            galleryView.Previous = carouselViewModel.CurrentIndex > 0 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex - 1) : null;
            galleryView.Current = CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex);
            galleryView.Next = carouselViewModel.CurrentIndex < collection.Count - 1 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex + 1) : null;
            galleryView.RefreshLayout();
        }
        
        private void Gallery_PreviousRequested(object? sender, EventArgs e)
        {
            if (sender is not GalleryView { BindingContext: CarouselPreviewerViewModel carouselViewModel } galleryView)
                return;
            
            if (carouselViewModel.CurrentIndex <= 0)
                return;

            carouselViewModel.CurrentIndex--;
            
            var oldIndex = carouselViewModel.CurrentIndex;
            var oldPrevious = carouselViewModel.Slides.ElementAtOrDefault(oldIndex - 1);
            var oldCurrent = carouselViewModel.Slides.ElementAtOrDefault(oldIndex);
            var oldNext = carouselViewModel.Slides.ElementAtOrDefault(oldIndex + 1);
            
            (oldNext as IDisposable)?.Dispose();
            (oldCurrent as IDisposable)?.Dispose();
            (oldPrevious as IAsyncInitialize)?.InitAsync();
            
            galleryView.Previous = carouselViewModel.CurrentIndex > 0 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex - 1) : null;
            galleryView.RefreshLayout();
        }

        private void Gallery_NextRequested(object? sender, EventArgs e)
        {
            if (sender is not GalleryView { BindingContext: CarouselPreviewerViewModel carouselViewModel } galleryView)
                return;

            if (carouselViewModel.CurrentIndex >= carouselViewModel.Slides.Count - 1)
                return;
		
            carouselViewModel.CurrentIndex++;
            
            var oldIndex = carouselViewModel.CurrentIndex;
            var oldPrevious = carouselViewModel.Slides.ElementAtOrDefault(oldIndex - 1);
            var oldCurrent = carouselViewModel.Slides.ElementAtOrDefault(oldIndex);
            var oldNext = carouselViewModel.Slides.ElementAtOrDefault(oldIndex + 1);
            
            (oldPrevious as IDisposable)?.Dispose();
            (oldCurrent as IDisposable)?.Dispose();
            (oldNext as IAsyncInitialize)?.InitAsync();
            
            galleryView.Next = carouselViewModel.CurrentIndex < carouselViewModel.Slides.Count - 1 ? CreateGalleryView(carouselViewModel, carouselViewModel.CurrentIndex + 1) : null;
            galleryView.RefreshLayout();
        }

        private View CreateGalleryView(CarouselPreviewerViewModel carouselViewModel, int index)
        {
            if (_galleryView is null)
                throw new NullReferenceException(nameof(_galleryView));

            switch (carouselViewModel.Slides[index])
            {
                case ImagePreviewerViewModel imageViewModel:
                {
                    var converter = (Resources["ImageToSourceConverter"] as ImageToSourceConverter)!;
                    var source = converter.Convert(imageViewModel.Source, typeof(ImageSource), null, CultureInfo.CurrentCulture) as ImageSource;
                    return new PanPinchContainer()
                    {
                        PanUpdatedCommand = _galleryView.PanUpdatedCommand,
                        Content = new Image()
                        {
                            Source = source
                        }
                    };
                }

                case VideoPreviewerViewModel videoViewModel:
                {
                    var converter = (Resources["MediaToSourceConverter"] as MediaToSourceConverter)!;
                    var source = converter.Convert(videoViewModel.Source, typeof(MediaSource), null, CultureInfo.CurrentCulture) as MediaSource;
                    return new MediaElement()
                    {
                        InputTransparent = true,
                        ShouldAutoPlay = true,
                        ShouldLoopPlayback = true,
                        Source = source
                    };
                }
                
                default:
                    return new ContentView();
            }
            
            // TODO: This does not work -- It seems that dispose is called at wrong times
            // return new ContentPresentation()
            // {
            //     Presentation = carouselViewModel.Slides[index],
            //     TemplateSelector = new PreviewerTemplateSelector()
            //     {
            //         ImageTemplate = Resources["ImageTemplate"] as DataTemplate,
            //         VideoTemplate = Resources["VideoTemplate"] as DataTemplate
            //     }
            // };
        }
    }
}

