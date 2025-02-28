using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class BrowserPage : ContentPage, IQueryAttributable, INavigator
    {
        public BrowserPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<BrowserViewModel>();
            if (ViewModel?.CurrentFolder is not null && ViewModel.InnerNavigator is MauiNavigationService navigationService)
            {
                navigationService.SetupNavigation(this);
                navigationService.Views.Add(ViewModel.CurrentFolder);
                navigationService.SetCurrentViewInternal(ViewModel.CurrentFolder);
            }

            OnPropertyChanged(nameof(ViewModel));
        }

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync(IViewDesignation? view)
        {
            if (view is not FolderViewModel folderViewModel)
                return false;

            // Make last item non-leading
            if (ViewModel?.Breadcrumbs?.LastOrDefault() is { } lastNavigationItem)
                lastNavigationItem.IsLeading = false;

            // Add navigated-to folder to Breadcrumb
            ViewModel?.Breadcrumbs?.Add(new(folderViewModel.Title, ViewModel.NavigateBreadcrumbCommand));

#if ANDROID
            // On Android: Navigate by changing current folder (i.e. ViewModel source)
            if (ViewModel is not null)
            {
                // Animate navigation
                await AnimateViewChangeAsync(folderViewModel);
                return true;
            }
#elif IOS
            // On iOS: Navigate whole page using shell
            // TODO: Set folder source in BrowserViewModel (to reuse the BrowserViewModel) and navigate the whole page instead of only the source
#endif
            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> GoBackAsync()
        {
            if (ViewModel?.InnerNavigator is not MauiNavigationService navigationService)
                return false;

            var index = navigationService.IndexInNavigation;
            if (navigationService.Views[Math.Max(--index, 0)] is not FolderViewModel folderViewModel)
                return false;

            // Animate navigation
            await AnimateViewChangeAsync(folderViewModel);

            // Remove last navigated-to breadcrumb
            var last = ViewModel?.Breadcrumbs?.LastOrDefault();
            if (last is not null)
                ViewModel?.Breadcrumbs?.Remove(last);

            // Make last navigated-to breadcrumb leading
            last = ViewModel?.Breadcrumbs?.LastOrDefault();
            if (last is not null)
                last.IsLeading = true;

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> GoForwardAsync()
        {
            if (ViewModel?.InnerNavigator is not MauiNavigationService navigationService)
                return false;

            var index = navigationService.IndexInNavigation;
            if (navigationService.Views[Math.Min(++index, Math.Max(navigationService.Views.Count, 0))] is not FolderViewModel folderViewModel)
                return false;

            // Animate navigation
            await AnimateViewChangeAsync(folderViewModel);

            // Make last navigated-to breadcrumb non-leading
            var last = ViewModel?.Breadcrumbs.LastOrDefault();
            if (last is not null)
                last.IsLeading = false;

            // Add new breadcrumb
            ViewModel?.Breadcrumbs?.Add(new(folderViewModel.Title, ViewModel.NavigateBreadcrumbCommand));

            return true;
        }

        /// <inheritdoc/>
        protected override bool OnBackButtonPressed()
        {
            if (ViewModel?.CurrentFolder is null)
                return base.OnBackButtonPressed();

            if (ViewModel.BaseFolder.Id == ViewModel.CurrentFolder.Folder.Id)
                return base.OnBackButtonPressed();

            _ = ViewModel.InnerNavigator.GoBackAsync();
            return true;
        }

        private async Task AnimateViewChangeAsync(FolderViewModel? folder)
        {
            if (ViewModel is null)
                return;

            await Browser.FadeTo(0.0d, 125u);
            ViewModel.CurrentFolder = folder;
            await Browser.FadeTo(1.0d, 125u);
        }

        public BrowserViewModel? ViewModel
        {
            get => (BrowserViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(BrowserViewModel), typeof(BrowserPage), null);
    }
}

