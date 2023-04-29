using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.UI.UserControls.BreadcrumbBar;
using System.Collections.ObjectModel;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    internal sealed partial class VaultDashboardPage : Page
    {
        public ObservableCollection<OrderedBreadcrumbBarItem> BreadcrumbItems { get; }

        public VaultDashboardPageViewModel? ViewModel
        {
            get => (VaultDashboardPageViewModel?)DataContext;
            set => DataContext = value;
        }

        public VaultDashboardPage()
        {
            AvaloniaXamlLoader.Load(this);
            BreadcrumbItems = new();
        }

        public override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is not VaultDashboardPageViewModel viewModel)
                return;

            ViewModel = viewModel;
            await Navigation.NavigateAsync(viewModel.CurrentPage, new EntranceNavigationTransition());
            BreadcrumbItems.Add(new(viewModel.VaultViewModel.VaultModel.VaultName, true));

            base.OnNavigatedTo(e);
        }
    }
}