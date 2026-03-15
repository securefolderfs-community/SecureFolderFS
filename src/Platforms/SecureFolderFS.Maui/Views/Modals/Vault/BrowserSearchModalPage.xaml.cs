using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
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
    public partial class BrowserSearchModalPage : BaseModalPage, IOverlayControl
    {
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;

        public BrowserSearchModalPage(INavigation sourceNavigation)
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
            if (ViewModel is not null)
                ViewModel.CloseRequested -= ViewModel_CloseRequested;

            ViewModel = viewable as BrowserSearchOverlayViewModel;
            if (ViewModel is not null)
                ViewModel.CloseRequested += ViewModel_CloseRequested;

            OnPropertyChanged(nameof(ViewModel));
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            _modalTcs.SetResult(Result.Success);
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (ViewModel is not null)
            {
                ViewModel.CloseRequested -= ViewModel_CloseRequested;
                ViewModel.Dispose();
            }

            _modalTcs.TrySetResult(Result.Success);
        }
        
        [RelayCommand]
        private async Task CloseAsync()
        {
            await HideAsync();
        }

        private async void ResultsCollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is SelectableItemsView selectableItemsView)
                selectableItemsView.SelectedItem = null;

            if (e.CurrentSelection.FirstOrDefault() is not SearchBrowserItemViewModel selectedItem)
                return;

            if (ViewModel?.OpenSearchResultCommand is null)
                return;

            await ViewModel.OpenSearchResultCommand.ExecuteAsync(selectedItem);
        }
        
        private async void SearchInput_SearchButtonPressed(object? sender, EventArgs e)
        {
            if (ViewModel is null)
                return;
            
            await ViewModel.SubmitQueryCommand.ExecuteAsync(SearchInput.Text);
        }

        private async void ViewModel_CloseRequested(object? sender, EventArgs e)
        {
            await HideAsync();
        }
        
        public BrowserSearchOverlayViewModel? ViewModel
        {
            get => (BrowserSearchOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(BrowserSearchOverlayViewModel), typeof(BrowserSearchModalPage));
    }
}
