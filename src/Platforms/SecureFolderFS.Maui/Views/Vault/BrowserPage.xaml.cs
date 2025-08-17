using System.ComponentModel;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.UserControls.Browser;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class BrowserPage : ContentPage, IQueryAttributable, INavigator
    {
        public BrowserPage(BrowserViewModel? viewModel = null)
        {
            ConfigureViewModel(viewModel);
            InitializeComponent();
            BindingContext = this;
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var viewModel = query.ToViewModel<BrowserViewModel>();
            ConfigureViewModel(viewModel);
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

            // Navigate by changing current folder (i.e. ViewModel source)
            if (ViewModel is not null)
            {
                // Animate navigation
                await AnimateViewChangeAsync(folderViewModel);
                return true;
            }

            // On iOS: Navigate whole page using shell
            // TODO: Set folder source in BrowserViewModel (to reuse the BrowserViewModel) and navigate the whole page instead of only the source

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

            // Remove last navigated-to breadcrumb
            var last = ViewModel?.Breadcrumbs?.LastOrDefault();
            if (last is not null)
                ViewModel?.Breadcrumbs?.Remove(last);

            // Make last navigated-to breadcrumb leading
            last = ViewModel?.Breadcrumbs?.LastOrDefault();
            if (last is not null)
                last.IsLeading = true;

            // Animate navigation
            await AnimateViewChangeAsync(folderViewModel);
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

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            if (ViewModel is not null)
                ViewModel.Layouts.PropertyChanged += Layouts_PropertyChanged;

            // OnAppearing is called elsewhere in navigation logic.
            base.OnAppearing();
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            if (ViewModel is not null)
                ViewModel.Layouts.PropertyChanged -= Layouts_PropertyChanged;

            ViewModel?.OnDisappearing();
            base.OnDisappearing();
        }

        private void ConfigureViewModel(BrowserViewModel? viewModel)
        {
            ViewModel = viewModel;
            if (ViewModel?.CurrentFolder is not null && ViewModel.InnerNavigator is MauiNavigationService navigationService)
            {
                navigationService.SetupNavigation(this);
                navigationService.Views.Add(ViewModel.CurrentFolder);
                navigationService.SetCurrentViewInternal(ViewModel.CurrentFolder);
            }

            OnPropertyChanged(nameof(ViewModel));
        }

        private async Task AnimateViewChangeAsync(FolderViewModel? folder)
        {
            if (ViewModel is null)
                return;

            await Browser.FadeTo(0.0d, 150U);
            ViewModel.CurrentFolder = folder;

            _ = Task.Delay(40).ContinueWith(async _ =>
            {
                await Browser.FadeTo(1.0d, 150U);
            }).Unwrap();
        }

        private async void Layouts_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(LayoutsViewModel.BrowserViewType))
                return;

            Browser.ItemsSource = null;
            await Task.Delay(100);
            Browser.SetBinding(BrowserControl.ItemsSourceProperty, $"{nameof(ViewModel)}.{nameof(BrowserViewModel.CurrentFolder)}.{nameof(FolderViewModel.Items)}");
        }

        public BrowserViewModel? ViewModel
        {
            get => (BrowserViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(BrowserViewModel), typeof(BrowserPage));
    }
}

