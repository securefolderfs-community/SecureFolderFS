using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.AvaloniaUI.UserControls.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.UserControls.BreadcrumbBar;
using System.Collections.ObjectModel;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    internal sealed partial class VaultDashboardPage : Page
    {
        public ObservableCollection<OrderedBreadcrumbBarItem> BreadcrumbItems { get; }

        public VaultDashboardPageViewModel ViewModel
        {
            get => (VaultDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultDashboardPage()
        {
            AvaloniaXamlLoader.Load(this);
            BreadcrumbItems = new();
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultDashboardPageViewModel viewModel)
            {
                ViewModel = viewModel;
                BreadcrumbItems.Add(new(viewModel.VaultViewModel.VaultModel.VaultName, true));
            }

            base.OnNavigatedTo(e);
        }

        public override void OnNavigatingFrom()
        {
            // Remove the reference to the NavigationControl so the page can get properly garbage collected
            ViewModel.DashboardNavigationService.ResetNavigation<FrameNavigationControl>();
            ViewModel.DashboardNavigationService.NavigationChanged -= DashboardNavigationService_NavigationChanged;
            Navigation.Dispose();

            base.OnNavigatingFrom();
        }

        private async void Navigation_Loaded(object sender, RoutedEventArgs e)
        {
            // Hook up navigation event
            ViewModel.DashboardNavigationService.NavigationChanged += DashboardNavigationService_NavigationChanged;

            // Initialize navigation
            if (ViewModel.DashboardNavigationService.SetupNavigation(Navigation, true))
            {
                var target = ViewModel.DashboardNavigationService.CurrentTarget ?? GetDefaultDashboardViewModel(); // Get current target or initialize default
                INavigationTarget GetDefaultDashboardViewModel()
                {
                    var controlsViewModel = new VaultControlsViewModel(ViewModel.UnlockedVaultViewModel, ViewModel.DashboardNavigationService, ViewModel.NavigationService);
                    return new VaultOverviewPageViewModel(ViewModel.UnlockedVaultViewModel, controlsViewModel, ViewModel.DashboardNavigationService);
                }

                await ViewModel.DashboardNavigationService.NavigateAsync(target);
            }
        }

        private async void DashboardNavigationService_NavigationChanged(object? sender, INavigationTarget? e)
        {
            var canGoBack = e switch
            {
                VaultOverviewPageViewModel => false,
                _ => true
            };

            if (canGoBack)
            {
                //GoBack.IsVisible = true;
                await ShowBackButtonStoryboard.BeginAsync();
                ShowBackButtonStoryboard.Stop();
            }
            else
            {
                await HideBackButtonStoryboard.BeginAsync();
                HideBackButtonStoryboard.Stop();
                //GoBack.IsVisible = false;
            }
        }
    }
}