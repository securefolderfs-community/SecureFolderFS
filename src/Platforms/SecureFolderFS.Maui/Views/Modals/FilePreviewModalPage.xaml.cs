using CommunityToolkit.Maui.Views;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
#endif

namespace SecureFolderFS.Maui.Views.Modals
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
            if ((Presentation.Content as ContentView)?.Content is not MediaElement mediaElement)
                return;

            mediaElement.Stop();
            mediaElement.Handler?.DisconnectHandler();
            mediaElement.Dispose();
            mediaElement.Source = null;
            
            _modalTcs.TrySetResult(Result.Success);
            base.OnDisappearing();
        }

        public PreviewerOverlayViewModel? ViewModel
        {
            get => (PreviewerOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PreviewerOverlayViewModel), typeof(FilePreviewModalPage), null);
    }
}

